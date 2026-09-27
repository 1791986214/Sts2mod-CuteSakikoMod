using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Ui;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace CuteSakikoMod.CuteSakikoModCode.Patches.Skin;

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
public static class SkinSelectCharacterPatch
{
    static void Postfix(CharacterModel characterModel)
    {
        var type = characterModel.GetType();
        var hasSkins = CharacterSkinRegistry.HasSkins(type);
        SkinUiContext.SetCurrentCharacter(hasSkins ? type : null);
    }
}

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuOpened))]
public static class SkinSubmenuOpenedPatch
{
    static void Postfix(NCharacterSelectScreen __instance)
    {
        var lobby = __instance.Lobby;
        if (lobby == null)
        {
            SkinUiContext.SetCurrentCharacter(null);
            return;
        }

        // ★ 进入角色选择界面：完全重建本机缓存（清掉上次大厅残留）
        SkinResolver.SyncFromLobby(lobby, forceResetLocal: true);

        var type = lobby.LocalPlayer.character?.GetType();
        var hasSkins = type != null && CharacterSkinRegistry.HasSkins(type);
        SkinUiContext.SetCurrentCharacter(hasSkins ? type : null);
    }
}