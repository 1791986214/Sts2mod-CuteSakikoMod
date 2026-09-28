using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace CuteSakikoMod.CuteSakikoModCode.Powers.Buff;

[RegisterPower]
public class ChordNextSkillCostReductionPower : CuteSakikoModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != Owner || card.Type != CardType.Skill)
            return false;
        var pileType = card.Pile?.Type;
        if (pileType != PileType.Hand && pileType != PileType.Play)
            return false;
        modifiedCost = originalCost - 1;
        if (modifiedCost < 0) modifiedCost = 0;
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Skill)
            return;
        var pileType = cardPlay.Card.Pile?.Type;
        if (pileType != PileType.Hand && pileType != PileType.Play)
            return;
        await PowerCmd.Decrement(this);
    }
}