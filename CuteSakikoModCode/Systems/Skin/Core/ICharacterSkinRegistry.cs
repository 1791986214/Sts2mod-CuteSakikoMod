
using Godot;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public interface ICharacterSkinRegistry
{
    Type CharacterType { get; }
    IReadOnlyList<CharacterSkinDefinition> AllSkins { get; }
}

public static class CharacterSkinRegistry
{
    private static readonly Dictionary<Type, ICharacterSkinRegistry> _registries = [];

    public static void Register(ICharacterSkinRegistry registry)
    {
        if (_registries.ContainsKey(registry.CharacterType))
        {
            GD.Print($"[SkinRegistry] Skip duplicate: {registry.CharacterType.Name}");
            return;
        }
        _registries[registry.CharacterType] = registry;
        GD.Print($"[SkinRegistry] Registered: {registry.CharacterType.Name} (skins={registry.AllSkins.Count})");
    }

    public static bool HasSkins(Type characterType) => _registries.ContainsKey(characterType);

    public static ICharacterSkinRegistry? ForCharacter(Type characterType)
        => _registries.GetValueOrDefault(characterType);

    public static IReadOnlyList<CharacterSkinDefinition>? AllSkinsFor(Type characterType)
        => ForCharacter(characterType)?.AllSkins;

    public static CharacterSkinDefinition? GetSkin(Type characterType, int index)
    {
        var list = AllSkinsFor(characterType);
        if (list == null || list.Count == 0) return null;
        return list[Math.Clamp(index, 0, list.Count - 1)];
    }

    public static IReadOnlyList<ICharacterSkinRegistry> GetAll() => _registries.Values.ToList();
}