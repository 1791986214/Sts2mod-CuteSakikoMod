using CuteSakikoMod.CuteSakikoModCode.Others;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Keywords;

namespace CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Uncommon;

public class PracticePractice() : CuteAnonCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CutesakiKeywords.NoNote.GetModCardKeyword()];

    public override int MaxUpgradeLevel => 999;

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("Notes", 3); }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        TriggerBanter();
        var amount = DynamicVars["Notes"].IntValue;
        var rng = Owner.RunState.Rng.CombatCardSelection;
        var noteTypes = new[] { CardType.Attack, CardType.Skill, CardType.Power };
        var chords = Owner.GetChords();
        if (chords == null) return;

        for (var i = 0; i < amount; i++)
        {
            var type = rng.NextItem(noteTypes);
            await chords.OnNoteGenerated(choiceContext, Owner, type);
        }
    }

    protected override void OnUpgrade()
    {
        var additionalNotes = 3 * CurrentUpgradeLevel;
        DynamicVars["Notes"].UpgradeValueBy(additionalNotes);
    }
}