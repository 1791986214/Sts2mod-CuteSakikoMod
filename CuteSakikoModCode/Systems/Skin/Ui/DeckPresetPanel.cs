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
    private static DeckViewModal? _currentDeckModal;

    private Label _deckNameLabel = null!;

    private bool _logicalVisible;
    private NGoldArrowButton _nextBtn = null!;
    private NGoldArrowButton _prevBtn = null!;

    public override async void _Ready()
    {
        GD.Print("[DeckPresetPanel] _Ready");

        Size = new Vector2(380, 110);
        MouseFilter = MouseFilterEnum.Pass;

        BuildUi();

        // ★ 等 UI 树布局稳定后再根据 InfoPanel 定位
        for (var i = 0; i < 3; i++)
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
        var shouldBeVisible = _logicalVisible && !SkinPanelVisibilityHelper.IsInspectScreenOpen();
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
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(bg);

        _prevBtn = SkinPreviewPanel.MakeArrowButton(
            "res://images/atlases/ui_atlas.sprites/settings_tiny_left_arrow.tres");
        _prevBtn.Position = new Vector2(8, 20);
        _prevBtn.Size = new Vector2(48, 64);
        AddChild(_prevBtn);
        _prevBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ => Shift(-1)));

        var viewBtn = new Button
        {
            Position = new Vector2(72, 20),
            Size = new Vector2(236, 48)
        };
        var viewLabel = new Label
        {
            // ★ 走本地化
            Text = new LocString("characters", "SKIN_VIEW_DECK").GetFormattedText(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            OffsetLeft = 0,
            OffsetTop = 0,
            OffsetRight = 0,
            OffsetBottom = 0,
            MouseFilter = MouseFilterEnum.Ignore
        };
        viewLabel.AddThemeFontSizeOverride("font_size", 20);
        viewBtn.AddChild(viewLabel);
        viewBtn.Pressed += OpenDeckView;
        AddChild(viewBtn);

        _nextBtn = SkinPreviewPanel.MakeArrowButton(
            "res://images/atlases/ui_atlas.sprites/settings_tiny_right_arrow.tres");
        _nextBtn.Position = new Vector2(324, 20);
        _nextBtn.Size = new Vector2(48, 64);
        AddChild(_nextBtn);
        _nextBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ => Shift(1)));

        _deckNameLabel = new Label
        {
            Position = new Vector2(0, 76),
            Size = new Vector2(380, 26),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _deckNameLabel.AddThemeFontSizeOverride("font_size", 18);
        AddChild(_deckNameLabel);
    }

    // ─────────────────────────────────────────────────────────
    // 定位：左边缘和 InfoPanel 对齐，紧贴其上，避开顶部头像
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

        var infoPanel = screen.GetNodeOrNull<Control>("InfoPanel");
        if (infoPanel == null)
        {
            GD.PrintErr("[DeckPresetPanel] RepositionToInfoPanel: InfoPanel null");
            return;
        }

        var rect = infoPanel.GetRect();
        var x = rect.Position.X;
        var y = rect.Position.Y - Size.Y - 16f;

        if (y < 8f)
            y = rect.Position.Y + rect.Size.Y + 8f;

        Position = new Vector2(x, y);
        GD.Print($"[DeckPresetPanel] Repositioned to ({x:F0},{y:F0}) infoPanel rect={rect}");
    }

    private void OnCurrentCharacterChanged(Type? _)
    {
        RefreshAndToggle();
        CallDeferred(nameof(RepositionToInfoPanel));
    }

    private void OnArtSkinChanged(Type _)
    {
        RefreshAndToggle();
    }

    private void OnDeckPresetChanged(Type _)
    {
        Refresh();
    }

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
            SkinDataStore.ModifyLobbyChoice(lobby, characterType,
                c => c.DeckPresetIndex = SkinResolver.GetLocalChoice(characterType).DeckPresetIndex);
        else if (RunManager.Instance?.DebugOnlyGetState() != null) SkinSyncService.Broadcast(characterType);

        SkinSystemEvents.RaiseDeckPresetChanged(characterType);
    }

    private void Refresh()
    {
        if (SkinUiContext.CurrentCharacterType is not { } characterType) return;
        var skin = SkinResolver.GetLocalSkin(characterType);
        if (skin == null || skin.DeckPresets.Count == 0) return;

        var canSwitch = skin.DeckPresets.Count > 1;
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

        if (_currentDeckModal != null && IsInstanceValid(_currentDeckModal))
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
            for (var i = 0; i < Mathf.Max(1, count); i++)
                cards.Add(model);
        }

        if (cards.Count == 0) return;

        try
        {
            NCard.InitPool();
        }
        catch
        {
        }

        try
        {
            NGridCardHolder.InitPool();
        }
        catch
        {
        }

        var host = (Node?)NGame.Instance ?? GetTree().Root;
        var viewportSize = GetViewportRect().Size;

        // ★ 不要设 ZIndex = 100！那会让 modal 盖住 hover tip。
        //   改用 tree order：把 modalRoot 插到 HoverTipsContainer 之前。
        var modalRoot = new DeckViewModal
        {
            Name = ModalRootName
        };
        _currentDeckModal = modalRoot;
        modalRoot.TreeExited += () =>
        {
            if (_currentDeckModal == modalRoot) _currentDeckModal = null;
        };
        host.AddChild(modalRoot);

        // ★ 关键：把 modalRoot 移到 HoverTipsContainer 之前，
        //   效果：DeckViewModal 盖住角色选择界面（皮肤面板），
        //         但不盖住 hover tip。
        if (NGame.Instance != null && host == NGame.Instance)
        {
            var hoverContainer = NGame.Instance.HoverTipsContainer;
            if (hoverContainer != null)
            {
                var hoverIdx = hoverContainer.GetIndex();
                if (hoverIdx >= 0)
                {
                    NGame.Instance.MoveChild(modalRoot, hoverIdx);
                    GD.Print($"[DeckPresetPanel] Moved modalRoot to index {hoverIdx} " +
                             $"(before HoverTipsContainer)");
                }
            }
            else
            {
                GD.PrintErr("[DeckPresetPanel] HoverTipsContainer is null; hover tip may be hidden");
            }
        }

        // 显式铺满整个游戏视口
        modalRoot.Position = Vector2.Zero;
        modalRoot.Size = viewportSize;
        modalRoot.MouseFilter = MouseFilterEnum.Stop;

        // 全屏半透明黑色遮罩，拦截所有下层点击
        var dim = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.75f),
            Position = Vector2.Zero,
            Size = viewportSize,
            MouseFilter = MouseFilterEnum.Stop
        };
        modalRoot.AddChild(dim);

        const float panelW = 1400f;
        const float panelH = 900f;
        var panelLeft = (viewportSize.X - panelW) / 2f;
        var panelTop = (viewportSize.Y - panelH) / 2f;

        var panel = new PanelContainer
        {
            Position = new Vector2(panelLeft, panelTop),
            Size = new Vector2(panelW, panelH),
            MouseFilter = MouseFilterEnum.Stop
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
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(title);

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
            ClipContents = false // ★ 卡牌 hover 放大时不裁剪
        };
        scroll.ScrollVertical = 0;
        vbox.AddChild(scroll);

        const float cardW = 240f;
        const float cardH = 338f;
        const float padding = 30f;
        const int columns = 5;

        var rows = (int)Mathf.Ceil((float)cards.Count / columns);
        var canvasW = columns * cardW + (columns - 1) * padding;
        var canvasH = rows * cardH + (rows - 1) * padding;

        var canvas = new Control
        {
            CustomMinimumSize = new Vector2(canvasW, canvasH),
            Size = new Vector2(canvasW, canvasH),
            ClipContents = false // ★ 不裁剪子节点
        };
        scroll.AddChild(canvas);

        for (var i = 0; i < cards.Count; i++)
        {
            var col = i % columns;
            var row = i / columns;
            var model = cards[i];

            var cardNode = NCard.Create(model);
            if (cardNode == null) continue;

            var holder = NGridCardHolder.Create(cardNode);
            if (holder == null) continue;

            var cx = col * (cardW + padding) + cardW / 2f;
            var cy = row * (cardH + padding) + cardH / 2f;
            holder.Position = new Vector2(cx, cy);

            canvas.AddChild(holder);
            cardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);

            var capturedIndex = i;
            holder.Connect(NCardHolder.SignalName.Pressed,
                Callable.From<NCardHolder>(_ => ShowInspect(cards, capturedIndex)));
        }

        // ── 关闭按钮：面板底部下方居中，方形圆角，白底黑字 ──
        const float closeBtnW = 140f;
        const float closeBtnH = 50f;
        var close = new Button
        {
            // ★ 走本地化
            Text = new LocString("characters", "SKIN_CLOSE").GetFormattedText(),
            Size = new Vector2(closeBtnW, closeBtnH),
            MouseFilter = MouseFilterEnum.Stop
        };
        var closeX = panelLeft + (panelW - closeBtnW) / 2f;
        var closeY = panelTop + panelH + 20f;
        if (closeY + closeBtnH > viewportSize.Y - 8f)
            closeY = viewportSize.Y - closeBtnH - 8f;
        close.Position = new Vector2(closeX, closeY);

        StyleBoxFlat MakeCloseStyle(Color bgColor)
        {
            var sb = new StyleBoxFlat { BgColor = bgColor };
            const int radius = 10;
            sb.CornerRadiusTopLeft = radius;
            sb.CornerRadiusTopRight = radius;
            sb.CornerRadiusBottomLeft = radius;
            sb.CornerRadiusBottomRight = radius;
            return sb;
        }

        close.AddThemeStyleboxOverride("normal", MakeCloseStyle(new Color(1f, 1f, 1f)));
        close.AddThemeStyleboxOverride("hover", MakeCloseStyle(new Color(0.95f, 0.95f, 0.95f)));
        close.AddThemeStyleboxOverride("pressed", MakeCloseStyle(new Color(0.85f, 0.85f, 0.85f)));
        close.AddThemeStyleboxOverride("focus", MakeCloseStyle(new Color(1f, 1f, 1f)));

        close.AddThemeColorOverride("font_color", Colors.Black);
        close.AddThemeColorOverride("font_hover_color", Colors.Black);
        close.AddThemeColorOverride("font_pressed_color", new Color(0.15f, 0.15f, 0.15f));
        close.AddThemeColorOverride("font_focus_color", Colors.Black);
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

        inspect.Open(cards, index);
        // DeckViewModal._Process 会自动检测 inspect 并隐藏自己
    }
}