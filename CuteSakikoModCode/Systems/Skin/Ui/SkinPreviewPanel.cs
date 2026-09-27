using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Networking;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Runs;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Ui;

public sealed partial class SkinPreviewPanel : Control
{
    private SubViewport _viewport = null!;
    private Label _nameLabel = null!;
    private Node2D? _currentVisuals;

    // ★ 逻辑可见性：inspect 打开时这个值不变，Visible 由 _Process 动态计算
    private bool _logicalVisible;

    public override void _Ready()
    {
        GD.Print("[SkinPreviewPanel] _Ready");

        Size = new Vector2(380, 400);
        Position = new Vector2(1400, 200);
        MouseFilter = MouseFilterEnum.Pass;

        BuildUi();

        SkinSystemEvents.CurrentCharacterChanged += OnCurrentCharacterChanged;
        SkinSystemEvents.ArtSkinChanged += OnArtSkinChanged;
        RefreshAndToggle();
    }

    public override void _ExitTree()
    {
        SkinSystemEvents.CurrentCharacterChanged -= OnCurrentCharacterChanged;
        SkinSystemEvents.ArtSkinChanged -= OnArtSkinChanged;
    }

    // ★ 每帧检查 inspect 是否打开，动态调整自己的可见性
    public override void _Process(double delta)
    {
        bool shouldBeVisible = _logicalVisible && !SkinPanelVisibilityHelper.IsInspectScreenOpen();
        if (Visible != shouldBeVisible)
            Visible = shouldBeVisible;
    }

    private void SetLogicalVisible(bool visible)
    {
        _logicalVisible = visible;
        Visible = visible && !SkinPanelVisibilityHelper.IsInspectScreenOpen();
    }

    private void BuildUi()
    {
        const float panelW = 380f;
        const float panelH = 400f;
        const float padX = 16f;
        const float padY = 16f;
        const float btnW = 48f;
        const float btnH = 64f;
        const float viewportW = 220f;
        const float viewportH = 320f;
        const float nameH = 28f;

        var bg = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.55f),
            Position = Vector2.Zero,
            Size = new Vector2(panelW, panelH),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(bg);

        float viewportX = padX + btnW + 8f;
        float viewportY = padY;

        var viewportContainer = new SubViewportContainer
        {
            Position = new Vector2(viewportX, viewportY),
            Size = new Vector2(viewportW, viewportH),
            Stretch = true,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(viewportContainer);

        _viewport = new SubViewport
        {
            TransparentBg = true,
            Size = new Vector2I((int)viewportW, (int)viewportH),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
        };
        viewportContainer.AddChild(_viewport);

        _viewport.AddChild(new Camera2D { Position = new Vector2(0, -160) });

        float btnY = viewportY + viewportH / 2 - btnH / 2;

        var prevBtn = MakeArrowButton("res://images/atlases/ui_atlas.sprites/settings_tiny_left_arrow.tres");
        prevBtn.Position = new Vector2(padX, btnY);
        prevBtn.Size = new Vector2(btnW, btnH);
        AddChild(prevBtn);
        prevBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ => Shift(-1)));

        float nextBtnX = viewportX + viewportW + 8f;
        var nextBtn = MakeArrowButton("res://images/atlases/ui_atlas.sprites/settings_tiny_right_arrow.tres");
        nextBtn.Position = new Vector2(nextBtnX, btnY);
        nextBtn.Size = new Vector2(btnW, btnH);
        AddChild(nextBtn);
        nextBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ => Shift(1)));

        _nameLabel = new Label
        {
            Position = new Vector2(0, viewportY + viewportH + 8f),
            Size = new Vector2(panelW, nameH),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _nameLabel.AddThemeFontSizeOverride("font_size", 20);
        AddChild(_nameLabel);
    }

    internal static NGoldArrowButton MakeArrowButton(string texturePath)
    {
        var btn = new NGoldArrowButton { Size = new Vector2(48, 64) };

        var mat = new ShaderMaterial { Shader = ResourceLoader.Load<Shader>("res://shaders/hsv.gdshader") };
        mat.SetShaderParameter("h", 1.0f);
        mat.SetShaderParameter("s", 1.0f);
        mat.SetShaderParameter("v", 1.0f);

        var icon = new TextureRect
        {
            Name = "TextureRect",
            Texture = ResourceLoader.Load<Texture2D>(texturePath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
            Material = mat,
            Position = new Vector2(0, 8),
            Size = new Vector2(48, 48),
        };
        btn.AddChild(icon);
        return btn;
    }

    private void OnCurrentCharacterChanged(Type? _) => RefreshAndToggle();
    private void OnArtSkinChanged(Type _) => Refresh();

    private void RefreshAndToggle()
    {
        var reg = SkinUiContext.CurrentRegistry;
        SetLogicalVisible(reg != null && reg.AllSkins.Count > 1);
        if (_logicalVisible) Refresh();
    }

    private void Shift(int delta)
    {
        if (SkinUiContext.CurrentCharacterType is not { } characterType) return;
        var reg = SkinUiContext.CurrentRegistry;
        if (reg == null) return;

        SkinResolver.ModifyLocalChoice(characterType, c =>
        {
            var count = reg.AllSkins.Count;
            c.ArtSkinIndex = (c.ArtSkinIndex + delta + count) % count;
            var newSkin = reg.AllSkins[c.ArtSkinIndex];
            c.DeckPresetIndex  = Math.Min(c.DeckPresetIndex,  newSkin.DeckPresets.Count - 1);
            c.RelicPresetIndex = Math.Min(c.RelicPresetIndex, newSkin.RelicPresets.Count - 1);
        });

        if (SkinUiContext.GetLobby(this) is { } lobby)
        {
            SkinDataStore.ModifyLobbyChoice(lobby, characterType,
                c => c.ArtSkinIndex = SkinResolver.GetLocalChoice(characterType).ArtSkinIndex);
        }
        else if (RunManager.Instance?.DebugOnlyGetState() != null)
        {
            SkinSyncService.Broadcast(characterType);
        }

        Refresh();
        SkinSystemEvents.RaiseArtSkinChanged(characterType);
    }

    private void Refresh()
    {
        if (SkinUiContext.CurrentCharacterType is not { } characterType) return;
        var skin = SkinResolver.GetLocalSkin(characterType);
        if (skin == null) return;

        _currentVisuals?.QueueFree();
        _currentVisuals = null;

        var packed = ResourceLoader.Load<PackedScene>(skin.Assets.VisualsPath);
        if (packed != null)
        {
            _currentVisuals = packed.Instantiate<Node2D>();
            _viewport.AddChild(_currentVisuals);
        }

        _nameLabel.Text = new LocString("characters", skin.DisplayNameKey).GetFormattedText();
    }
}