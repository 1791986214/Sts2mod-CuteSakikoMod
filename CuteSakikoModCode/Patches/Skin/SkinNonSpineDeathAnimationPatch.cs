using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CuteSakikoMod.CuteSakikoModCode.Patches.Skin;

/// <summary>
/// 原版 NCreature.StartDeathAnim 里触发 Dead 的代码被包在 if (_spineAnimator != null) 里。
/// 非 Spine 皮肤（ob）的 _spineAnimator 为 null → SetAnimationTrigger("Dead") 不被调用。
/// 这里在 StartDeathAnim 执行前手动触发一次，让 RitsuLib 路由到我们的状态机。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
[HarmonyPriority(900)]
public static class SkinNonSpineDeathAnimationPatch
{
    static void Prefix(NCreature __instance, bool shouldRemove)
    {
        var entity = __instance.Entity;
        if (entity == null || !entity.IsPlayer) return;

        var player = entity.Player;
        if (player == null) return;

        var charType = player.Character.GetType();
        if (!CharacterSkinRegistry.HasSkins(charType)) return;

        // Spine 皮肤走原版
        if (__instance.HasSpineAnimation) return;

        __instance.SetAnimationTrigger("Dead");
        GD.Print($"[SkinPatch] NonSpineDeath: forced Dead trigger netId={player.NetId}");
    }
}