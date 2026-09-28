using System.Reflection;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace CuteSakikoMod.CuteSakikoModCode.Patches.Skin;

/// <summary>
///     让角色选择界面的 HP/Gold 文本跟着皮肤/遗物预设变化。
///     _hp / _gold 是私有字段，反射拿一次。
/// </summary>
public static class SkinCharacterStatsRefreshPatch
{
    private static FieldInfo? _hpField;
    private static FieldInfo? _goldField;

    public static void Init()
    {
        // 皮肤变化 → 刷新当前角色选择界面上的 HP/Gold 文本
        SkinSystemEvents.ArtSkinChanged += _ => RefreshAll();
        SkinSystemEvents.RelicPresetChanged += _ => RefreshAll();
        SkinSystemEvents.CurrentCharacterChanged += _ => RefreshAll();
    }

    private static void RefreshAll()
    {
        var screen = FindCurrentScreen();
        if (screen == null) return;
        Refresh(screen);
    }

    private static void Refresh(NCharacterSelectScreen screen)
    {
        _hpField ??= AccessTools.Field(typeof(NCharacterSelectScreen), "_hp");
        _goldField ??= AccessTools.Field(typeof(NCharacterSelectScreen), "_gold");

        var lobby = screen.Lobby;
        if (lobby == null) return;

        // LocalPlayer 是 struct，直接取；character 是引用字段，可以判 null
        var charModel = lobby.LocalPlayer.character;
        if (charModel == null) return;

        var hp = charModel.StartingHp;
        var gold = charModel.StartingGold;

        if (_hpField?.GetValue(screen) is MegaLabel hpLabel)
            hpLabel.SetTextAutoSize($"{hp}/{hp}");
        if (_goldField?.GetValue(screen) is MegaLabel goldLabel)
            goldLabel.SetTextAutoSize($"{gold}");
    }

    private static NCharacterSelectScreen? FindCurrentScreen()
    {
        // NGame.Instance 下所有 children 里找
        var game = NGame.Instance;
        if (game == null) return null;
        return FindRecursive(game);
    }

    private static NCharacterSelectScreen? FindRecursive(Node n)
    {
        if (n is NCharacterSelectScreen s) return s;
        foreach (var c in n.GetChildren())
            if (FindRecursive(c) is { } found)
                return found;
        return null;
    }
}