using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public static class SkinDataStore
{
    private static bool _registered;
    public static PlayerRunSavedData<SkinSelectionState> Data { get; private set; } = null!;

    public static void Register(string modId)
    {
        if (_registered) return;
        _registered = true;

        using (RitsuLibFramework.BeginModDataRegistration(modId))
        {
            var store = RitsuLibFramework.GetRunSavedDataStore(modId);
            Data = store.RegisterPerPlayer(
                "skin_selection_state",
                () => new SkinSelectionState(),
                new RunSavedDataOptions
                {
                    WritePolicy = RunSavedDataWritePolicy.WhenSet,
                    SyncLobbyOnChange = true
                });
        }
    }

    private static string Key(Type characterType)
    {
        return characterType.FullName ?? characterType.Name;
    }

    public static void ModifyLobbyChoice(StartRunLobby lobby, Type characterType, Action<CharacterSkinChoice> mutator)
    {
        var key = Key(characterType);
        Data.Lobby.Modify(lobby, lobby.NetService.NetId, state =>
        {
            if (!state.Choices.TryGetValue(key, out var choice))
            {
                choice = new CharacterSkinChoice();
                state.Choices[key] = choice;
            }

            mutator(choice);
        });
    }

    /// <summary>从大厅暂存读任意玩家的选择（含远端）。不存在则返回默认。</summary>
    public static CharacterSkinChoice GetLobbyChoice(StartRunLobby lobby, ulong netId, Type characterType)
    {
        if (lobby == null) return new CharacterSkinChoice();
        if (!Data.Lobby.TryGet(lobby, netId, out var state) || state == null)
            return new CharacterSkinChoice();
        var key = Key(characterType);
        return state.Choices.TryGetValue(key, out var choice)
            ? choice.Clone()
            : new CharacterSkinChoice();
    }

    public static CharacterSkinChoice GetRunChoice(Player player, Type characterType)
    {
        var state = Data.Get(player);
        if (state != null && state.Choices.TryGetValue(Key(characterType), out var choice))
            return choice.Clone();
        return new CharacterSkinChoice();
    }

    public static CharacterSkinChoice GetRunChoice(RunState run, ulong netId, Type characterType)
    {
        var player = run.Players.FirstOrDefault(p => p.NetId == netId);
        if (player == null) return new CharacterSkinChoice();
        return GetRunChoice(player, characterType);
    }

    /// <summary>检查某玩家在某角色类型上是否在大厅数据里已有记录。</summary>
    public static bool HasLobbyChoice(StartRunLobby lobby, ulong netId, Type characterType)
    {
        if (lobby == null) return false;
        if (!Data.Lobby.TryGet(lobby, netId, out var state) || state == null) return false;
        return state.Choices.ContainsKey(Key(characterType));
    }
}