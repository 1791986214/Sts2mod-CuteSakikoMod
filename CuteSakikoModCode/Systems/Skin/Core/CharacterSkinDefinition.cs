using System.Collections.Generic;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public sealed record CharacterSkinDefinition(
    string Id,
    string DisplayNameKey,
    int StartingHp,
    int StartingGold,
    SkinAssets Assets,
    IReadOnlyList<DeckPreset> DeckPresets,
    IReadOnlyList<RelicPreset> RelicPresets)
{
    /// <summary>
    /// 用共享预设构造皮肤定义。默认继承 presets 里的牌组/遗物；
    /// 只在需要覆盖时才传 Override 参数。
    /// </summary>
    public static CharacterSkinDefinition FromPresets(
        string id,
        string displayNameKey,
        int startingHp,
        int startingGold,
        SkinAssets assets,
        CharacterPresetSet presets,
        IReadOnlyList<DeckPreset>? deckPresetsOverride = null,
        IReadOnlyList<RelicPreset>? relicPresetsOverride = null)
        => new(
            id,
            displayNameKey,
            startingHp,
            startingGold,
            assets,
            deckPresetsOverride ?? presets.DeckPresets,
            relicPresetsOverride ?? presets.RelicPresets);
}