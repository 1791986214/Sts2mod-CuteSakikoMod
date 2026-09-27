using Godot;
using MegaCrit.Sts2.Core.Nodes;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Ui;

/// <summary>
/// 检测原版 NInspectCardScreen 是否“真正处于打开状态”。
///
/// 不能用 Visible：Close() 里会先播 0.25s 淡出、最后才把 Visible 置 false。
/// 不能用 Modulate.a：Open() 里用 From(0.0f) 会在下一帧才生效，从 Click 到 tween
/// 真正开始之间有一帧残留旧值，导致判断抖动。
///
/// 唯一同步、无残留的信号是 MouseFilter：
///   Open() 第二行 → MouseFilter = Stop
///   Close() 第一行 → MouseFilter = Ignore
/// </summary>
public static class SkinPanelVisibilityHelper
{
    public static bool IsInspectScreenOpen()
    {
        var inspect = NGame.Instance?.GetInspectCardScreen();
        if (inspect == null
            || !GodotObject.IsInstanceValid(inspect)
            || !inspect.IsInsideTree()
            || !inspect.Visible)
        {
            return false;
        }

        return inspect.MouseFilter == Control.MouseFilterEnum.Stop;
    }
}