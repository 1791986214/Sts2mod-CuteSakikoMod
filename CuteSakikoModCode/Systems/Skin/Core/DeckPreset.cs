namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public sealed record DeckPreset(
    string Id,
    string DisplayNameKey,
    IReadOnlyList<(Type CardType, int Count)> Cards);

public sealed record RelicPreset(
    string Id,
    string DisplayNameKey,
    IReadOnlyList<Type> RelicTypes);