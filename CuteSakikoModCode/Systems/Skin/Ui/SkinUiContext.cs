using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using Godot;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Ui;

public static class SkinUiContext
{
    private static Type? _currentCharacterType;

    public static Type? CurrentCharacterType => _currentCharacterType;

    public static void SetCurrentCharacter(Type? characterType)
    {
        if (_currentCharacterType == characterType) return;
        _currentCharacterType = characterType;
        SkinSystemEvents.RaiseCurrentCharacterChanged(characterType);
    }

    public static ICharacterSkinRegistry? CurrentRegistry
        => _currentCharacterType is { } t ? CharacterSkinRegistry.ForCharacter(t) : null;

    public static StartRunLobby? GetLobby(Node node)
    {
        Node? n = node;
        while (n != null && n is not NCharacterSelectScreen) n = n.GetParent();
        return (n as NCharacterSelectScreen)?.Lobby;
    }
}