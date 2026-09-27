using System;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Scaffolding.Godot;

namespace CuteSakikoMod.CuteSakikoModCode.Patches.Skin;

[HarmonyPatch(typeof(NCreature), nameof(NCreature.Create))]
public static class SkinCreatureCreatePatch
{
    private static readonly System.Reflection.PropertyInfo? VisualsProp =
        AccessTools.Property(typeof(NCreature), "Visuals");

    static void Postfix(Creature entity, NCreature __result)
    {
        if (__result == null || entity == null || !entity.IsPlayer) return;
        var player = entity.Player;
        if (player == null) return;

        // ★ 不再区分本机/远端
        var charType = player.Character.GetType();
        if (!CharacterSkinRegistry.HasSkins(charType)) return;

        var skin = SkinResolver.GetSkinForPlayer(player.NetId, charType);
        if (skin == null) return;

        NCreatureVisuals? newVisuals;
        try
        {
            newVisuals = RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(
                skin.Assets.VisualsPath);
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[SkinPatch] CreateFromScenePath failed: " +
                        $"{skin.Assets.VisualsPath}: {ex.Message}");
            return;
        }
        if (newVisuals == null) return;

        __result.Visuals?.QueueFreeSafely();
        VisualsProp?.SetValue(__result, newVisuals);
    }
}