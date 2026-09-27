using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;

namespace CuteSakikoMod.CuteSakikoModCode.Patches.Skin;

/// <summary>
/// 跑局顶栏玩家状态头像：按该玩家的皮肤替换 icon 贴图。
/// 只对远端玩家生效；本机玩家由 RitsuLib 的 AssetProfile 处理。
/// </summary>
[HarmonyPatch(typeof(NMultiplayerPlayerState), nameof(NMultiplayerPlayerState._Ready))]
public static class SkinMultiplayerPlayerStatePatch
{
    private static readonly System.Reflection.FieldInfo? CharacterIconField =
        AccessTools.Field(typeof(NMultiplayerPlayerState), "_characterIcon");

    static void Postfix(NMultiplayerPlayerState __instance)
    {
        var player = __instance.Player;
        if (player == null) return;

        var charType = player.Character.GetType();
        if (!CharacterSkinRegistry.HasSkins(charType)) return;

        var skin = SkinResolver.GetSkinForPlayer(player.NetId, charType);
        if (skin == null) return;

        if (CharacterIconField?.GetValue(__instance) is TextureRect icon)
        {
            var tex = ResourceLoader.Load<Texture2D>(skin.Assets.IconTexturePath);
            if (tex != null) icon.Texture = tex;
        }
    }
}