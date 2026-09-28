namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public sealed record CharacterSkinDefinition(
    string Id,
    string DisplayNameKey,
    SkinAssets Assets,
    IReadOnlyList<DeckPreset> DeckPresets,
    IReadOnlyList<RelicPreset> RelicPresets)
{
    public static CharacterSkinDefinition FromPresets(
        string id,
        string displayNameKey,
        SkinAssets assets,
        CharacterPresetSet presets,
        IReadOnlyList<DeckPreset>? deckPresetsOverride = null,
        IReadOnlyList<RelicPreset>? relicPresetsOverride = null)
    {
        return new CharacterSkinDefinition(
            id,
            displayNameKey,
            assets,
            deckPresetsOverride ?? presets.DeckPresets,
            relicPresetsOverride ?? presets.RelicPresets);
    }
}