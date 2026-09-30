using CuteSakikoMod.CuteSakikoModCode.Others;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Keywords;

namespace CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Rare;

public class PerfectPlay : CuteAnonCard
{
    public PerfectPlay() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CutesakiKeywords.NoNote.GetModCardKeyword()];

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("ChordCount", 6); }
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromKeyword(CutesakiKeywords.EquippedChords.GetModCardKeyword());
            yield return HoverTipFactory.FromKeyword(CutesakiKeywords.LearnedChords.GetModCardKeyword());
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        TriggerBanter();
        var chords = Owner.GetChords();
        if (chords == null) return;

        var learned = chords.GetLearnedChords().ToList();
        if (learned.Count == 0) return;

        var targetCount = DynamicVars["ChordCount"].IntValue;

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        var selected = new List<string>(targetCount);
        for (var i = 0; i < targetCount; i++)
            selected.Add(learned[rng.NextInt(learned.Count)]);

        foreach (var chordId in selected)
        {
            if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
                break;

            await ChordNoteSystem.PlayChordAsync(Owner, chordId, choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        DynamicVars["ChordCount"].UpgradeValueBy(4);
    }
}