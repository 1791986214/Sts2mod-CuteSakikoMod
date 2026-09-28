using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Networking;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Ui;

public sealed partial class RelicSwitchPanel : Control
{
    private bool _logicalVisible;
    private NGoldArrowButton _nextBtn = null!;
    private NGoldArrowButton _prevBtn = null!;

    public override async void _Ready()
    {
        GD.Print("[RelicSwitchPanel] _Ready");

        Position = Vector2.Zero;
        Size = Vector2.Zero;
        MouseFilter = MouseFilterEnum.Ignore;

        BuildUi();

        for (var i = 0; i < 3; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        AttachToRelicBox();

        SkinSystemEvents.CurrentCharacterChanged += OnCurrentCharacterChanged;
        SkinSystemEvents.ArtSkinChanged += OnArtSkinChanged;
        SkinSystemEvents.RelicPresetChanged += OnRelicPresetChanged;
        RefreshAndToggle();
    }

    public override void _ExitTree()
    {
        SkinSystemEvents.CurrentCharacterChanged -= OnCurrentCharacterChanged;
        SkinSystemEvents.ArtSkinChanged -= OnArtSkinChanged;
        SkinSystemEvents.RelicPresetChanged -= OnRelicPresetChanged;
    }

    public override void _Process(double delta)
    {
        var shouldBeVisible = _logicalVisible && !SkinPanelVisibilityHelper.IsInspectScreenOpen();
        if (_prevBtn.Visible != shouldBeVisible) _prevBtn.Visible = shouldBeVisible;
        if (_nextBtn.Visible != shouldBeVisible) _nextBtn.Visible = shouldBeVisible;
    }

    private void SetLogicalVisible(bool visible)
    {
        _logicalVisible = visible;
        var effective = visible && !SkinPanelVisibilityHelper.IsInspectScreenOpen();
        if (_prevBtn != null) _prevBtn.Visible = effective;
        if (_nextBtn != null) _nextBtn.Visible = effective;
    }

    private void BuildUi()
    {
        _prevBtn = SkinPreviewPanel.MakeArrowButton(
            "res://images/atlases/ui_atlas.sprites/settings_tiny_left_arrow.tres");
        _prevBtn.Size = new Vector2(48, 64);
        AddChild(_prevBtn);
        _prevBtn.Connect(NClickableControl.SignalName.Released,
            Callable.From<NClickableControl>(_ => Shift(-1)));

        _nextBtn = SkinPreviewPanel.MakeArrowButton(
            "res://images/atlases/ui_atlas.sprites/settings_tiny_right_arrow.tres");
        _nextBtn.Size = new Vector2(48, 64);
        AddChild(_nextBtn);
        _nextBtn.Connect(NClickableControl.SignalName.Released,
            Callable.From<NClickableControl>(_ => Shift(1)));
    }

    private void AttachToRelicBox()
    {
        var screen = FindScreen();
        if (screen == null)
        {
            GD.PrintErr("[RelicSwitchPanel] FindScreen() null");
            return;
        }

        var relicBox = screen.GetNodeOrNull<Control>("InfoPanel/VBoxContainer/Relic");
        if (relicBox == null)
        {
            GD.PrintErr("[RelicSwitchPanel] Relic box not found");
            return;
        }

        relicBox.ClipContents = false;
        _prevBtn.GetParent()?.RemoveChild(_prevBtn);
        _nextBtn.GetParent()?.RemoveChild(_nextBtn);
        relicBox.AddChild(_prevBtn);
        relicBox.AddChild(_nextBtn);

        RepositionToRelicBox();
    }

    private void RepositionToRelicBox()
    {
        var screen = FindScreen();
        if (screen == null) return;

        var relicBox = screen.GetNodeOrNull<Control>("InfoPanel/VBoxContainer/Relic");
        var icon = relicBox?.GetNodeOrNull<Control>("Icon");
        var desc = relicBox?.GetNodeOrNull<Control>("Description");
        if (relicBox == null || icon == null || desc == null) return;

        var iconRect = icon.GetRect();
        var descRect = desc.GetRect();

        // ★ 用 relicBox 的垂直中心当统一基准，两个箭头同高
        const float btnH = 64f;
        var centerY = relicBox.Size.Y / 2f - btnH / 2f;

        _prevBtn.Position = new Vector2(
            iconRect.Position.X - 8 - 48,
            centerY);

        _nextBtn.Position = new Vector2(
            descRect.Position.X + descRect.Size.X + 8,
            centerY);
    }

    private void OnCurrentCharacterChanged(Type? _)
    {
        RefreshAndToggle();
        Callable.From(RepositionToRelicBox).CallDeferred();
    }

    private void OnArtSkinChanged(Type _)
    {
        RefreshAndToggle();
    }

    private void OnRelicPresetChanged(Type _)
    {
        Refresh();
    }

    private void RefreshAndToggle()
    {
        var reg = SkinUiContext.CurrentRegistry;
        var hasRegistry = reg != null && reg.AllSkins.Count > 0;

        if (!hasRegistry)
        {
            SetLogicalVisible(false);
            return;
        }

        var skin = SkinResolver.GetLocalSkin(SkinUiContext.CurrentCharacterType!);
        var canSwitch = skin != null && skin.RelicPresets.Count > 1;

        SetLogicalVisible(canSwitch);

        if (canSwitch) Refresh();
    }

    private void Shift(int delta)
    {
        if (SkinUiContext.CurrentCharacterType is not { } characterType) return;
        var skin = SkinResolver.GetLocalSkin(characterType);
        if (skin == null || skin.RelicPresets.Count <= 1) return;

        SkinResolver.ModifyLocalChoice(characterType, c =>
            c.RelicPresetIndex = (c.RelicPresetIndex + delta + skin.RelicPresets.Count) % skin.RelicPresets.Count);

        if (SkinUiContext.GetLobby(this) is { } lobby)
            SkinDataStore.ModifyLobbyChoice(lobby, characterType,
                c => c.RelicPresetIndex = SkinResolver.GetLocalChoice(characterType).RelicPresetIndex);
        else if (RunManager.Instance?.DebugOnlyGetState() != null) SkinSyncService.Broadcast(characterType);

        SkinSystemEvents.RaiseRelicPresetChanged(characterType);
    }

    private void Refresh()
    {
        var screen = FindScreen();
        if (screen == null) return;
        if (SkinUiContext.CurrentCharacterType is not { } characterType) return;

        var skin = SkinResolver.GetLocalSkin(characterType);
        if (skin == null || skin.RelicPresets.Count == 0) return;

        var idx = Math.Clamp(SkinResolver.GetLocalChoice(characterType).RelicPresetIndex,
            0, skin.RelicPresets.Count - 1);
        var preset = skin.RelicPresets[idx];
        if (preset.RelicTypes.Count == 0) return;

        var relic = ModelDb.GetById<RelicModel>(ModelDb.GetId(preset.RelicTypes[0]));

        var title = screen.GetNode<MegaRichTextLabel>("InfoPanel/VBoxContainer/Relic/Name/RichTextLabel");
        var desc = screen.GetNode<MegaRichTextLabel>("InfoPanel/VBoxContainer/Relic/Description");
        var icon = screen.GetNode<TextureRect>("InfoPanel/VBoxContainer/Relic/Icon");
        var outline = screen.GetNode<TextureRect>("InfoPanel/VBoxContainer/Relic/Icon/Outline");

        title.Text = relic.Title.GetFormattedText();
        desc.Text = relic.DynamicDescription.GetFormattedText();
        icon.Texture = relic.Icon;
        outline.Texture = relic.IconOutline;
        icon.SelfModulate = Colors.White;
        outline.SelfModulate = StsColors.halfTransparentBlack;

        Callable.From(RepositionToRelicBox).CallDeferred();
    }

    private NCharacterSelectScreen? FindScreen()
    {
        Node? n = this;
        while (n != null && n is not NCharacterSelectScreen) n = n.GetParent();
        return n as NCharacterSelectScreen;
    }
}