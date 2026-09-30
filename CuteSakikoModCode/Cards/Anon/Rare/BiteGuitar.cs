using CuteSakikoMod.CuteSakikoModCode.Others;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Keywords;

namespace CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Rare;

public class BiteGuitar : CuteAnonCard
{
    public BiteGuitar() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromKeyword(CutesakiKeywords.EquippedChords.GetModCardKeyword()); }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(15m, ValueProp.Move) };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        TriggerBanter();

        var damage = DynamicVars.Damage.IntValue;
        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var chords = Owner.GetChords();
        if (chords == null) return;

        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        var rng = combat.RunState.Rng.CombatCardSelection;
        var weightedPool = new (CardType type, double weight)[]
        {
            (CardType.Attack, 0.48),
            (CardType.Skill, 0.48),
            (CardType.Power, 0.04)
        };

        foreach (var chordId in chords.GetEquippedChordIds())
        {
            if (!ChordManager.AllChords.TryGetValue(chordId, out var def)) continue;

            var shuffled = def.NoteSequence.Select(_ => PickRandomWeighted(rng, weightedPool)).ToList();
            ChordSequenceModifierHelper.SetCardModifier(Owner, chordId, new ShuffleNotesModifier(shuffled));
        }

        ChordNoteUIManager.UpdateNoteDisplay(Owner);
        ChordNoteUIManager.UpdateStoredChordDisplay(Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }

    private static CardType PickRandomWeighted(Rng rng, (CardType type, double weight)[] pool)
    {
        var totalWeight = pool.Sum(w => w.weight);
        var roll = rng.NextFloat() * totalWeight;
        double cumulative = 0;
        foreach (var item in pool)
        {
            cumulative += item.weight;
            if (roll <= cumulative) return item.type;
        }

        return pool.Last().type;
    }
}