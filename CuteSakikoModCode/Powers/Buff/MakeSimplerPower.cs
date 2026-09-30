using CuteSakikoMod.CuteSakikoModCode.Relics.Anon.Starter;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace CuteSakikoMod.CuteSakikoModCode.Powers.Buff;

public class MakeSimplerPower : CuteSakikoModPower, IChordSequenceModifierProvider
{
    // 缓存修饰符结果，键为和弦ID
    private readonly Dictionary<string, List<ChordSequenceModifier>> _cachedModifiers = new();
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public IEnumerable<ChordCategory>? AffectedCategories => null;

    public IEnumerable<ChordSequenceModifier> GetModifiers(Creature owner, ChordDefinition chordDef)
    {
        if (Amount <= 0) yield break;

        // 有缓存直接返回
        if (_cachedModifiers.TryGetValue(chordDef.Id, out var cached))
        {
            foreach (var mod in cached)
                yield return mod;
            yield break;
        }

        var chords = owner.Player?.GetChords();
        if (chords == null) yield break;
        
        var allChordIds = chords.GetEquippedChordIds();
        if (!allChordIds.Contains(chordDef.Id)) yield break;

        var noteCount = chordDef.NoteSequence.Length;
        if (noteCount == 0) yield break;

        var replaceCount = Math.Min(Amount, noteCount);

        var combatState = owner.CombatState;
        if (combatState == null) yield break;

        var rng = combatState.RunState.Rng.Niche;

        var indices = Enumerable.Range(0, noteCount).ToList();
        for (var i = indices.Count - 1; i > 0; i--)
        {
            var j = rng.NextInt(i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        var newModifiers = new List<ChordSequenceModifier>();
        for (var i = 0; i < replaceCount; i++)
        {
            var mod = new ReplaceNoteModifier(indices[i], Entry.AnyNote);
            newModifiers.Add(mod);
            yield return mod;
        }

        // 存入缓存
        _cachedModifiers[chordDef.Id] = newModifiers;
    }

    // 当层数改变时清除缓存，保证效果更新
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
        if (power == this)
            _cachedModifiers.Clear();
    }
}