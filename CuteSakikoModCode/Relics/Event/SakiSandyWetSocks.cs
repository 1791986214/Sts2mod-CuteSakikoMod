using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace CuteSakikoMod.CuteSakikoModCode.Relics.Event;

public class SakiSandyWetSocks : CuteSakikoEventRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromPower<IntangiblePower>(); }
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature)) return;
        if (Owner.PlayerCombatState.TurnNumber > 1) return;

        Flash();
        await PowerCmd.Apply<IntangiblePower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, null);
    }
}