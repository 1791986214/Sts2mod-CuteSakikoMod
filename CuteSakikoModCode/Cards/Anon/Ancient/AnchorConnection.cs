using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Ancient;

public class AnchorConnection() : CuteAnonCard(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selectedChordIds = await ChordCmd.SelectChords(choiceContext, Owner, 2);
        if (selectedChordIds.Count < 2) return;

        if (Owner.GetChords() == null) return;

        foreach (var chordId in selectedChordIds)
        {
            if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
                break;

            await ChordNoteSystem.PlayChordAsync(Owner, chordId, choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}