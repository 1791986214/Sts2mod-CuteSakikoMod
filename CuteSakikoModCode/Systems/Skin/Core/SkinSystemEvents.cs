namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public static class SkinSystemEvents
{
    public static event Action<Type?>? CurrentCharacterChanged;
    public static event Action<Type>? ArtSkinChanged;
    public static event Action<Type>? DeckPresetChanged;
    public static event Action<Type>? RelicPresetChanged;

    public static void RaiseCurrentCharacterChanged(Type? characterType)
        => CurrentCharacterChanged?.Invoke(characterType);

    public static void RaiseArtSkinChanged(Type characterType)
        => ArtSkinChanged?.Invoke(characterType);

    public static void RaiseDeckPresetChanged(Type characterType)
        => DeckPresetChanged?.Invoke(characterType);

    public static void RaiseRelicPresetChanged(Type characterType)
        => RelicPresetChanged?.Invoke(characterType);
}