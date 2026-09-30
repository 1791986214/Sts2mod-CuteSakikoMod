using CuteSakikoMod.CuteSakikoModCode.Nodes;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Chord;

public static class ChordCmd
{
    // ChordCmd.cs 中的 SelectChords 方法（完整替换）
    public static async Task<List<string>> SelectChords(
        PlayerChoiceContext context,
        Player player,
        int count,
        int multiplier = 0) // 新增 multiplier 参数，默认为 0 表示基础值
    {
        var runManager = RunManager.Instance;
        var sync = runManager.PlayerChoiceSynchronizer;
        var choiceId = sync.ReserveChoiceId(player);

        await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.CancelPlayCardActions);

        List<int> chordIndexes = null;

        if (runManager.NetService.NetId == player.NetId)
        {
            var screen = new ChordLibraryScreen();
            var selectedIds = await screen.ShowSelection(count, multiplier);
            if (selectedIds != null && selectedIds.Count == count)
                chordIndexes = selectedIds
                    .Select(id => ChordManager.AllChordsList.FindIndex(c => c.Id == id))
                    .ToList();
            else
                chordIndexes = new List<int>();

            sync.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromIndexes(chordIndexes));
        }
        else
        {
            var remoteResult = await sync.WaitForRemoteChoice(player, choiceId);
            chordIndexes = remoteResult.AsIndexes();
        }

        await context.SignalPlayerChoiceEnded();

        if (chordIndexes == null || chordIndexes.Count == 0)
            return new List<string>();

        return chordIndexes.Select(i => ChordManager.AllChordsList[i].Id).ToList();
    }

    public static bool AddRandomBonusChord(ChordStorageCapability chords)
    {
        if (chords?.HostPlayer == null) return false;

        var owned = new HashSet<string>(chords.GetAllEquippedChords());
        foreach (var id in chords.GetLearnedChords())
            owned.Add(id);

        var allPools = new List<string>();
        allPools.AddRange(ChordManager.GetLearnableChordIds(ChordCategory.Major));
        allPools.AddRange(ChordManager.GetLearnableChordIds(ChordCategory.Minor));
        allPools.AddRange(ChordManager.GetLearnableChordIds(ChordCategory.Dominant));

        var available = allPools.Where(id => !owned.Contains(id)).ToList();
        if (available.Count == 0) return false;

        var player = chords.HostPlayer;
        var newChord = player.RunState.Rng.UpFront.NextItem(available);
        chords.AddBonusChord(newChord);
        chords.LearnChord(newChord);
        return true;
    }

    public static int AddRandomTemporaryChords(ChordStorageCapability chords, int targetCount)
    {
        if (chords?.HostPlayer == null) return 0;

        var existing = chords.GetTemporaryChords().ToList();
        var currentCount = existing.Count;
        if (currentCount >= targetCount) return 0;

        var needed = targetCount - currentCount;
        var added = 0;
        var player = chords.HostPlayer;
        var rng = player.RunState.Rng.UpFront;

        var allPools = new List<string>();
        allPools.AddRange(ChordManager.GetLearnableChordIds(ChordCategory.Major));
        allPools.AddRange(ChordManager.GetLearnableChordIds(ChordCategory.Minor));
        allPools.AddRange(ChordManager.GetLearnableChordIds(ChordCategory.Dominant));

        var owned = new HashSet<string>(chords.GetAllEquippedChords());
        foreach (var id in chords.GetLearnedChords()) owned.Add(id);
        foreach (var id in existing) owned.Add(id);

        var available = allPools.Where(id => !owned.Contains(id)).ToList();

        while (added < needed && available.Count > 0)
        {
            var chordId = rng.NextItem(available);
            chords.AddTemporaryChord(chordId);
            chords.LearnChord(chordId);
            available.Remove(chordId);
            added++;
        }

        if (added < needed)
        {
            var learned = chords.GetLearnedChords().ToList();
            if (learned.Count == 0) return added;

            while (added < needed)
            {
                var chordId = rng.NextItem(learned);
                chords.AddTemporaryChord(chordId);
                added++;
            }
        }

        return added;
    }

    public static List<string> LearnRandomChords(ChordStorageCapability chords, int count)
    {
        if (chords?.HostPlayer == null) return new List<string>();

        var pool = new List<string>();
        foreach (var cat in new[] { ChordCategory.Major, ChordCategory.Minor, ChordCategory.Dominant })
            pool.AddRange(ChordManager.GetLearnableChordIds(cat));

        var alreadyKnown = new HashSet<string>(chords.GetLearnedChords());
        var available = pool.Where(id => !alreadyKnown.Contains(id) && !ChordManager.AllChords[id].IsTemporaryOnly)
            .ToList();

        if (available.Count == 0) return new List<string>();

        var rng = chords.HostPlayer.RunState.Rng.CombatCardGeneration;
        var toLearn = available.OrderBy(_ => rng.NextFloat()).Take(count).ToList();

        foreach (var chordId in toLearn)
            chords.LearnChord(chordId);

        return toLearn;
    }

    public static async Task AddRandomImprovisedChord(Player player, PlayerChoiceContext context)
    {
        var pool = new List<string>();
        pool.AddRange(ChordManager.GetLearnableChordIds(ChordCategory.Major));
        pool.AddRange(ChordManager.GetLearnableChordIds(ChordCategory.Minor));
        pool.AddRange(ChordManager.GetLearnableChordIds(ChordCategory.Dominant));
        if (pool.Count == 0) return;

        var randomChordId = player.RunState.Rng.CombatCardSelection.NextItem(pool);

        ChordNoteSystem.Activate(player);
        await ChordNoteSystem.AddStoredChordAsync(player, randomChordId, 1, context);
    }
}