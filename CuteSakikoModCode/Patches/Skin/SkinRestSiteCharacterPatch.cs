using System;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using STS2RitsuLib.Scaffolding.Godot;

namespace CuteSakikoMod.CuteSakikoModCode.Patches.Skin;

/// <summary>
/// 休息处角色：用该玩家自己选择的皮肤替换。
///
/// ★ 必须 Priority.First，因为 RitsuLib 自己在 NRestSiteCharacter.Create 上挂了
///   一个 Prefix + return false 的补丁（NRestSiteCharacterCreateProceduralPatch），
///   会挡住后面注册的所有 Prefix。我们抢在它前面跑，返回 false 设置 __result，
///   它就不会执行。对于非我们角色，我们返回 true，让 RitsuLib 的补丁接管。
/// </summary>
[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.Create))]
[HarmonyPriority(Priority.First)]
public static class SkinRestSiteCharacterPatch
{
    private static readonly System.Reflection.FieldInfo? PlayerField =
        FindPlayerBackingField();

    private static readonly System.Reflection.FieldInfo? CharacterIndexField =
        AccessTools.Field(typeof(NRestSiteCharacter), "_characterIndex");

    private static System.Reflection.FieldInfo? FindPlayerBackingField()
    {
        var t = typeof(NRestSiteCharacter);
        // 优先按已知命名找，找不到就按字段类型兜底
        var byName = AccessTools.Field(t, "<Player>k__BackingField")
                     ?? AccessTools.Field(t, "_player");
        if (byName != null) return byName;

        foreach (var f in t.GetFields(System.Reflection.BindingFlags.Instance
                                      | System.Reflection.BindingFlags.Public
                                      | System.Reflection.BindingFlags.NonPublic))
        {
            if (f.FieldType == typeof(Player)) return f;
        }
        return null;
    }

    static bool Prefix(Player player, int characterIndex, ref NRestSiteCharacter? __result)
    {
        // 调试日志：确认我们的 Prefix 被调用（如果这行也没出现，就是优先级仍然输给 RitsuLib）
        GD.Print($"[SkinPatch] RestSite Prefix: netId={player?.NetId}");

        if (player == null) return true;

        var charType = player.Character.GetType();
        if (!CharacterSkinRegistry.HasSkins(charType)) return true;

        var skin = SkinResolver.GetSkinForPlayer(player.NetId, charType);
        if (skin == null) return true;

        if (PlayerField == null)
        {
            GD.PrintErr("[SkinPatch] RestSite: Player backing field not found");
            return true;
        }

        try
        {
            var node = RitsuGodotNodeFactories.CreateFromScenePath<NRestSiteCharacter>(
                skin.Assets.RestSitePath);
            if (node == null)
            {
                GD.PrintErr($"[SkinPatch] RestSite: factory null for {skin.Assets.RestSitePath}");
                return true;
            }

            PlayerField.SetValue(node, player);
            CharacterIndexField?.SetValue(node, characterIndex);
            __result = node;

            GD.Print($"[SkinPatch] RestSite: netId={player.NetId} local={SkinResolver.LocalNetId} " +
                     $"path={skin.Assets.RestSitePath}");
            return false;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SkinPatch] RestSite patch failed: {ex}");
            return true;
        }
    }
}