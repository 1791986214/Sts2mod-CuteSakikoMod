using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace CuteSakikoMod.CuteSakikoModCode.Patches.Skin;

/// <summary>
/// 大厅队友头像：直接读 lobby 暂存里的皮肤选择（远端玩家的选择已经通过
/// SyncLobbyOnChange 同步过来）。NRemoteLobbyPlayer 只代表队友，本机由原版显示。
/// </summary>
[HarmonyPatch(typeof(NRemoteLobbyPlayer), "RefreshVisuals")]
public static class SkinLobbyPlayerVisualPatch
{
    private static readonly System.Reflection.FieldInfo? CharacterIconField =
        AccessTools.Field(typeof(NRemoteLobbyPlayer), "_characterIcon");
    private static readonly System.Reflection.FieldInfo? CharacterField =
        AccessTools.Field(typeof(NRemoteLobbyPlayer), "_character");

    static void Postfix(NRemoteLobbyPlayer __instance)
    {
        if (CharacterField?.GetValue(__instance) is not CharacterModel character) return;
        var charType = character.GetType();
        if (!CharacterSkinRegistry.HasSkins(charType)) return;

        var screen = FindScreen(__instance);
        var lobby = screen?.Lobby;
        if (lobby == null) return;

        var choice = SkinDataStore.GetLobbyChoice(lobby, __instance.PlayerId, charType);
        var skin = CharacterSkinRegistry.GetSkin(charType, choice.ArtSkinIndex);
        if (skin == null) return;

        if (CharacterIconField?.GetValue(__instance) is TextureRect icon)
        {
            var tex = ResourceLoader.Load<Texture2D>(skin.Assets.IconTexturePath);
            if (tex != null) icon.Texture = tex;
        }
    }

    private static NCharacterSelectScreen? FindScreen(Node n)
    {
        while (n != null && n is not NCharacterSelectScreen) n = n.GetParent();
        return n as NCharacterSelectScreen;
    }
}