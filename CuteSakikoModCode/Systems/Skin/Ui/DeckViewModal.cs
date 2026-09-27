using Godot;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Ui;

/// <summary>
/// 卡组查看界面的根节点。
/// - 全屏半透明遮罩由 DeckPresetPanel 里的 ColorRect 负责
/// - 本类自身用 MouseFilter.Stop 拦截点击
/// - inspect 打开时自动隐藏，inspect 开始关闭时立即淡入
/// - 首次出现 / 重新出现 / 关闭都带渐变
/// 尺寸与位置由外部（DeckPresetPanel.OpenDeckView）显式设置。
/// </summary>
public sealed partial class DeckViewModal : Control
{
    private const float FirstAppearDuration = 0.22f;
    private const float ReappearDuration    = 0.10f; // inspect 关闭后重新出现，更早/更快
    private const float FadeOutDuration     = 0.18f;

    private Tween? _fadeTween;
    private bool   _wasVisible = true;
    private bool   _closing;

    public override void _Ready()
    {
        // 自己拦截：整块 modal 层不要漏给下层 UI
        MouseFilter = MouseFilterEnum.Stop;

        // 注意：不调用 SetAnchorsPreset，Size/Position 由外部显式设置

        Modulate = new Color(1, 1, 1, 0); // 初始全透明
        PlayFadeIn(FirstAppearDuration);
    }

    private void PlayFadeIn(float duration)
    {
        _fadeTween?.Kill();
        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(this, "modulate:a", 1.0f, duration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    /// <summary>带渐变地关闭自己（关闭按钮/外部调用都走这里）。</summary>
    public void CloseSelf()
    {
        if (_closing) return;
        _closing = true;

        _fadeTween?.Kill();
        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(this, "modulate:a", 0.0f, FadeOutDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);
        _fadeTween.TweenCallback(Callable.From(QueueFree));
    }

    public override void _Process(double delta)
    {
        if (_closing) return;

        bool inspectVisible  = SkinPanelVisibilityHelper.IsInspectScreenOpen();
        bool shouldBeVisible = !inspectVisible;

        if (_wasVisible == shouldBeVisible) return;
        _wasVisible = shouldBeVisible;

        if (shouldBeVisible)
        {
            // inspect 开始关闭 → 立即淡入，不再等 0.25s 淡出跑完
            Visible  = true;
            Modulate = new Color(1, 1, 1, 0);
            PlayFadeIn(ReappearDuration);
        }
        else
        {
            // inspect 打开 → 立刻隐藏，避免层级打架
            _fadeTween?.Kill();
            Visible  = false;
            Modulate = new Color(1, 1, 1, 1);
        }
    }
}