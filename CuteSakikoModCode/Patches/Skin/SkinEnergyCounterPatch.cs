using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CuteSakikoMod.CuteSakikoModCode.Patches.Skin;

[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter.Create))]
public static class SkinEnergyCounterPatch
{
    private static bool Prefix(Player player, ref NEnergyCounter? __result)
    {
        if (player == null) return true;

        // ★ 不再区分本机/远端
        var charType = player.Character.GetType();
        if (!CharacterSkinRegistry.HasSkins(charType)) return true;

        var skin = SkinResolver.GetSkinForPlayer(player.NetId, charType);
        if (skin == null) return true;

        try
        {
            var scene = PreloadManager.Cache.GetScene(skin.Assets.EnergyCounterPath);
            if (scene == null) return true;

            var counter = scene.Instantiate<NEnergyCounter>();
            AccessTools.Field(typeof(NEnergyCounter), "_player")?.SetValue(counter, player);
            __result = counter;
            return false;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SkinPatch] EnergyCounter patch failed: {ex.Message}");
            return true;
        }
    }
}