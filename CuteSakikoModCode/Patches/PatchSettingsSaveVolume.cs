using CuteSakikoMod.CuteSakikoModCode.Systems;
using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;

namespace CuteSakikoMod.CuteSakikoModCode.Patches;

[HarmonyPatch(typeof(SettingsSave), "set_VolumeMaster")]
public static class PatchSettingsSaveVolumeMaster
{
    private static void Postfix()
    {
        AudioManager.RefreshMusicVolume();
    }
}

[HarmonyPatch(typeof(SettingsSave), "set_VolumeBgm")]
public static class PatchSettingsSaveVolumeBgm
{
    private static void Postfix()
    {
        AudioManager.RefreshMusicVolume();
    }
}