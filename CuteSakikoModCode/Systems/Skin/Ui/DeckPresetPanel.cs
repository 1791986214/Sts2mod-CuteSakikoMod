using System.Collections.Generic;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Networking;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Ui;

public sealed partial class DeckPresetPanel : Control
{
    private const string ModalRootName = "SakiDeckViewModalRoot";

    private Label _deckNameLabel = null!;
    private NGoldArrowButton _prevBtn = null!;
    private NGoldArrowButton _nextBtn = null!;

    private bool _logicalVisible;
    private static DeckViewModal? _currentDeckModal;

    public override async void _Ready()
    {
        GD.Print("[DeckPresetPanel] _Ready");

        Size = new Vector2(380, 110);
        MouseFilter = MouseFilterEnum.Pass;

        BuildUi();

        // ★ 等 UI 树布局稳定后再根据 InfoPanel 定位
        for (int i = 0; i < 3; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        RepositionToInfoPanel();

        SkinSystemEvents.CurrentCharacterChanged += OnCurrentCharacterChanged;
        SkinSystemEvents.ArtSkinChanged += OnArtSkinChanged;
        SkinSystemEvents.DeckPresetChanged += OnDeckPresetChanged;
        RefreshAndToggle();
    }

    public override void _ExitTree()
    {
        SkinSystemEvents.CurrentCharacterChanged -= OnCurrentCharacterChanged;
        SkinSystemEvents.ArtSkinChanged -= OnArtSkinChanged;
        SkinSystemEvents.DeckPresetChanged -= OnDeckPresetChanged;
    }

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
        var bg = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.55f),
            Position = Vector2.Zero,
            Size = new Vector2(380, 110),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(bg);

        _prevBtn = SkinPreviewPanel.MakeArrowButton("res://images/atlases/ui_atlas.sprites/settings_tiny_left_arrow.tres");
        _prevBtn.Position = new Vector2(8, 20);
        _prevBtn.Size = new Vector2(48, 64);
        AddChild(_prevBtn);
        _prevBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ => Shift(-1)));

        var viewBtn = new Button
        {
            Position = new Vector2(72, 20),
            Size = new Vector2(236, 48),
        };
        var viewLabel = new Label
        {
            Text = "查看卡组",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            OffsetLeft = 0,
            OffsetTop = 0,
            OffsetRight = 0,
            OffsetBottom = 0,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        viewLabel.AddThemeFontSizeOverride("font_size", 20);
        viewBtn.AddChild(viewLabel);
        viewBtn.Pressed += OpenDeckView;
        AddChild(viewBtn);

        _nextBtn = SkinPreviewPanel.MakeArrowButton("res://images/atlases/ui_atlas.sprites/settings_tiny_right_arrow.tres");
        _nextBtn.Position = new Vector2(324, 20);
        _nextBtn.Size = new Vector2(48, 64);
        AddChild(_nextBtn);
        _nextBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ => Shift(1)));

        _deckNameLabel = new Label
        {
            Position = new Vector2(0, 76),
            Size = new Vector2(380, 26),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _deckNameLabel.AddThemeFontSizeOverride("font_size", 18);
        AddChild(_deckNameLabel);
    }

    // ─────────────────────────────────────────────────────────
    // ★ 定位：左边缘和 InfoPanel 对齐，紧贴其上，避开顶部头像
    // ─────────────────────────────────────────────────────────
    private NCharacterSelectScreen? FindScreen()
    {
        Node? n = this;
        while (n != null && n is not NCharacterSelectScreen) n = n.GetParent();
        return n as NCharacterSelectScreen;
    }

    private void RepositionToInfoPanel()
    {
        var screen = FindScreen();
        if (screen == null)
        {
            GD.PrintErr("[DeckPresetPanel] RepositionToInfoPanel: screen null");
            return;
        }

        // NCharacterSelectScreen._Ready 里用这个路径查的 InfoPanel：
        //   this._infoPanel = this.GetNode<Godot.Control>((NodePath) "InfoPanel");
        var infoPanel = screen.GetNodeOrNull<Control>("InfoPanel");
        if (infoPanel == null)
        {
            GD.PrintErr("[DeckPresetPanel] RepositionToInfoPanel: InfoPanel null");
            return;
        }

        var rect = infoPanel.GetRect();
        float x = rect.Position.X;
        float y = rect.Position.Y - Size.Y - 16f;   // 贴在 InfoPanel 上方，留 8px 间隙

        // 保底：如果上方空间不够（顶到屏幕顶部），放到 InfoPanel 下方
        if (y < 8f)
            y = rect.Position.Y + rect.Size.Y + 8f;

        Position = new Vector2(x, y);
        GD.Print($"[DeckPresetPanel] Repositioned to ({x:F0},{y:F0}) infoPanel rect={rect}");
    }

    private void OnCurrentCharacterChanged(Type? _)
    {
        RefreshAndToggle();
        // 这样写更明确，避免 Callable.From 对 Action 的隐式推断
        CallDeferred(nameof(RepositionToInfoPanel));
    }

    private void OnArtSkinChanged(Type _) => RefreshAndToggle();
    private void OnDeckPresetChanged(Type _) => Refresh();

    private void RefreshAndToggle()
    {
        var reg = SkinUiContext.CurrentRegistry;
        SetLogicalVisible(reg != null && reg.AllSkins.Count > 0);
        if (_logicalVisible) Refresh();
    }

    private void Shift(int delta)
    {
        if (SkinUiContext.CurrentCharacterType is not { } characterType) return;
        var skin = SkinResolver.GetLocalSkin(characterType);
        if (skin == null || skin.DeckPresets.Count <= 1) return;

        SkinResolver.ModifyLocalChoice(characterType, c =>
            c.DeckPresetIndex = (c.DeckPresetIndex + delta + skin.DeckPresets.Count) % skin.DeckPresets.Count);

        if (SkinUiContext.GetLobby(this) is { } lobby)
        {
            SkinDataStore.ModifyLobbyChoice(lobby, characterType,
                c => c.DeckPresetIndex = SkinResolver.GetLocalChoice(characterType).DeckPresetIndex);
        }
        else if (RunManager.Instance?.DebugOnlyGetState() != null)
        {
            SkinSyncService.Broadcast(characterType);
        }

        SkinSystemEvents.RaiseDeckPresetChanged(characterType);
    }

    private void Refresh()
    {
        if (SkinUiContext.CurrentCharacterType is not { } characterType) return;
        var skin = SkinResolver.GetLocalSkin(characterType);
        if (skin == null || skin.DeckPresets.Count == 0) return;

        bool canSwitch = skin.DeckPresets.Count > 1;
        _prevBtn.Visible = canSwitch;
        _nextBtn.Visible = canSwitch;

        var idx = Math.Clamp(SkinResolver.GetLocalChoice(characterType).DeckPresetIndex,
            0, skin.DeckPresets.Count - 1);
        _deckNameLabel.Text = new LocString("characters", skin.DeckPresets[idx].DisplayNameKey)
            .GetFormattedText();
    }

    private void OpenDeckView()
    {
        if (SkinUiContext.CurrentCharacterType is not { } characterType) return;
        var skin = SkinResolver.GetLocalSkin(characterType);
        if (skin == null || skin.DeckPresets.Count == 0) return;

        if (_currentDeckModal != null && GodotObject.IsInstanceValid(_currentDeckModal))
        {
            _currentDeckModal.QueueFree();
            _currentDeckModal = null;
        }

        var idx = Math.Clamp(SkinResolver.GetLocalChoice(characterType).DeckPresetIndex,
            0, skin.DeckPresets.Count - 1);
        var preset = skin.DeckPresets[idx];

        var cards = new List<CardModel>();
        foreach (var (cardType, count) in preset.Cards)
        {
            var model = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
            for (int i = 0; i < Mathf.Max(1, count); i++)
                cards.Add(model);
        }
        if (cards.Count == 0) return;

        try { NCard.InitPool(); } catch { }
        try { NGridCardHolder.InitPool(); } catch { }

        var host = (Node?)NGame.Instance ?? GetTree().Root;

        // ★ 拿到视口尺寸（游戏内部的 1920x1080 逻辑分辨率，不含窗口黑边）
        var viewportSize = GetViewportRect().Size;

        var modalRoot = new DeckViewModal
        {
            Name = ModalRootName,
            ZIndex = 100,
        };
        _currentDeckModal = modalRoot;
        modalRoot.TreeExited += () =>
        {
            if (_currentDeckModal == modalRoot) _currentDeckModal = null;
        };
        host.AddChild(modalRoot);

        // ★ 显式铺满整个游戏视口（不靠 anchors，不靠父尺寸）
        modalRoot.Position = Vector2.Zero;
        modalRoot.Size = viewportSize;
        modalRoot.MouseFilter = MouseFilterEnum.Stop;

        // 全屏半透明黑色遮罩，拦截所有下层点击
        var dim = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.75f),
            Position = Vector2.Zero,
            Size = viewportSize,
            MouseFilter = MouseFilterEnum.Stop,
        };
        modalRoot.AddChild(dim);

        const float panelW = 1400f;
        const float panelH = 900f;
        float panelLeft = (viewportSize.X - panelW) / 2f;
        float panelTop = (viewportSize.Y - panelH) / 2f;

        var panel = new PanelContainer
        {
            Position = new Vector2(panelLeft, panelTop),
            Size = new Vector2(panelW, panelH),
            MouseFilter = MouseFilterEnum.Stop,
        };
        modalRoot.AddChild(panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);
        panel.AddChild(margin);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        margin.AddChild(vbox);

        var title = new Label
        {
            Text = new LocString("characters", preset.DisplayNameKey).GetFormattedText(),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        title.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(title);

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
        };
        scroll.ScrollVertical = 0;
        vbox.AddChild(scroll);

        const float cardW = 240f;
        const float cardH = 338f;
        const float padding = 30f;
        const int columns = 5;

        int rows = (int)Mathf.Ceil((float)cards.Count / columns);
        float canvasW = columns * cardW + (columns - 1) * padding;
        float canvasH = rows * cardH + (rows - 1) * padding;

        var canvas = new Control
        {
            CustomMinimumSize = new Vector2(canvasW, canvasH),
            Size = new Vector2(canvasW, canvasH),
        };
        scroll.AddChild(canvas);

        for (int i = 0; i < cards.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            var model = cards[i];

            var cardNode = NCard.Create(model);
            if (cardNode == null) continue;

            var holder = NGridCardHolder.Create(cardNode);
            if (holder == null) continue;

            float cx = col * (cardW + padding) + cardW / 2f;
            float cy = row * (cardH + padding) + cardH / 2f;
            holder.Position = new Vector2(cx, cy);

            canvas.AddChild(holder);
            cardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);

            int capturedIndex = i;
            holder.Connect(NCardHolder.SignalName.Pressed,
                Callable.From<NCardHolder>(_ => ShowInspect(cards, capturedIndex)));
        }

        // ── 关闭按钮：面板底部下方居中，方形圆角，白底黑字 ──
        const float closeBtnW = 140f;
        const float closeBtnH = 50f;
        var close = new Button
        {
            Text = "关闭",
            Size = new Vector2(closeBtnW, closeBtnH),
            MouseFilter = MouseFilterEnum.Stop,
        };
        float closeX = panelLeft + (panelW - closeBtnW) / 2f;
        float closeY = panelTop + panelH + 20f;
        // 保底：不要跑出视口底部
        if (closeY + closeBtnH > viewportSize.Y - 8f)
            closeY = viewportSize.Y - closeBtnH - 8f;
        close.Position = new Vector2(closeX, closeY);

        StyleBoxFlat MakeCloseStyle(Color bgColor)
        {
            var sb = new StyleBoxFlat { BgColor = bgColor };
            const int radius = 10; // 方形圆角
            sb.CornerRadiusTopLeft     = radius;
            sb.CornerRadiusTopRight    = radius;
            sb.CornerRadiusBottomLeft  = radius;
            sb.CornerRadiusBottomRight = radius;
            return sb;
        }

        close.AddThemeStyleboxOverride("normal",  MakeCloseStyle(new Color(1f, 1f, 1f, 1f)));
        close.AddThemeStyleboxOverride("hover",   MakeCloseStyle(new Color(0.95f, 0.95f, 0.95f, 1f)));
        close.AddThemeStyleboxOverride("pressed", MakeCloseStyle(new Color(0.85f, 0.85f, 0.85f, 1f)));
        close.AddThemeStyleboxOverride("focus",   MakeCloseStyle(new Color(1f, 1f, 1f, 1f)));

        close.AddThemeColorOverride("font_color",         Colors.Black);
        close.AddThemeColorOverride("font_hover_color",   Colors.Black);
        close.AddThemeColorOverride("font_pressed_color", new Color(0.15f, 0.15f, 0.15f));
        close.AddThemeColorOverride("font_focus_color",   Colors.Black);
        close.AddThemeFontSizeOverride("font_size", 22);

        close.Pressed += () => modalRoot.CloseSelf();
        modalRoot.AddChild(close);

        GD.Print($"[DeckPresetPanel] Opened deck view: {preset.DisplayNameKey} ({cards.Count} cards)");
    }

    private static void ShowInspect(List<CardModel> cards, int index)
    {
        var inspect = NGame.Instance?.GetInspectCardScreen();
        if (inspect == null)
        {
            GD.PrintErr("[DeckPresetPanel] NInspectCardScreen not available");
            return;
        }
        inspect.Open(cards, index, false);
        // DeckViewModal._Process 会自动检测 inspect 并隐藏自己
    }
}