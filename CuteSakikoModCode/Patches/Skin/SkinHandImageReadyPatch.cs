using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace CuteSakikoMod.CuteSakikoModCode.Patches.Skin;

/// <summary>
/// 多人手势（指/石头/布/剪刀）：按该玩家的皮肤替换贴图。
/// 只改 texture 字段，不破坏节点结构；非我们角色直接退。
/// </summary>
[HarmonyPatch(typeof(NHandImage), nameof(NHandImage._Ready))]
public static class SkinHandImageReadyPatch
{
    private static readonly System.Reflection.FieldInfo? TextureRectField =
        AccessTools.Field(typeof(NHandImage), "_textureRect");

    static void Postfix(NHandImage __instance)
    {
        var player = __instance.Player;
        if (player == null) return;

        var charType = player.Character.GetType();
        if (!CharacterSkinRegistry.HasSkins(charType)) return;

        var skin = SkinResolver.GetSkinForPlayer(player.NetId, charType);
        if (skin == null) return;

        if (TextureRectField?.GetValue(__instance) is TextureRect tr)
        {
            var tex = ResourceLoader.Load<Texture2D>(skin.Assets.ArmPointingPath);
            if (tex != null) tr.Texture = tex;
        }
    }
}

[HarmonyPatch(typeof(NHandImage), "SetTextureToFightMove")]
public static class SkinHandImageFightPatch
{
    private static readonly System.Reflection.FieldInfo? TextureRectField =
        AccessTools.Field(typeof(NHandImage), "_textureRect");

    static void Postfix(NHandImage __instance, RelicPickingFightMove move)
    {
        var player = __instance.Player;
        if (player == null) return;

        var charType = player.Character.GetType();
        if (!CharacterSkinRegistry.HasSkins(charType)) return;

        var skin = SkinResolver.GetSkinForPlayer(player.NetId, charType);
        if (skin == null) return;

        string? path = move switch
        {
            RelicPickingFightMove.Rock     => skin.Assets.ArmRockPath,
            RelicPickingFightMove.Paper    => skin.Assets.ArmPaperPath,
            RelicPickingFightMove.Scissors => skin.Assets.ArmScissorsPath,
            _ => null,
        };
        if (string.IsNullOrEmpty(path)) return;

        if (TextureRectField?.GetValue(__instance) is TextureRect tr)
        {
            var tex = ResourceLoader.Load<Texture2D>(path);
            if (tex != null) tr.Texture = tex;
        }
    }
}