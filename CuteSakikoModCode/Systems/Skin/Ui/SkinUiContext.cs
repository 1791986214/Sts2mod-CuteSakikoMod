using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;
using Godot;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Ui;

public static class SkinUiContext
{
    public static Type? CurrentCharacterType { get; private set; }

    public static ICharacterSkinRegistry? CurrentRegistry
        => CurrentCharacterType is { } t ? CharacterSkinRegistry.ForCharacter(t) : null;

    public static void SetCurrentCharacter(Type? characterType)
    {
        if (CurrentCharacterType == characterType) return;
        CurrentCharacterType = characterType;
        SkinSystemEvents.RaiseCurrentCharacterChanged(characterType);
    }

    public static StartRunLobby? GetLobby(Node node)
    {
        var n = node;
        while (n != null && n is not NCharacterSelectScreen) n = n.GetParent();
        return (n as NCharacterSelectScreen)?.Lobby;
    }
}