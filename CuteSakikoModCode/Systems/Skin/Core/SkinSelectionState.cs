namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public sealed class SkinSelectionState
{
    // key = Type.FullName
    public Dictionary<string, CharacterSkinChoice> Choices { get; set; } = [];
}

public sealed class CharacterSkinChoice
{
    public int ArtSkinIndex { get; set; }
    public int DeckPresetIndex { get; set; }
    public int RelicPresetIndex { get; set; }

    public CharacterSkinChoice Clone()
    {
        return new CharacterSkinChoice
        {
            ArtSkinIndex = ArtSkinIndex,
            DeckPresetIndex = DeckPresetIndex,
            RelicPresetIndex = RelicPresetIndex
        };
    }
}