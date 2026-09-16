using System.Reflection;
using System.Threading.Tasks;
using CuteSakikoMod.CuteSakikoModCode.Systems;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interactions.RightClick;

namespace CuteSakikoMod.CuteSakikoModCode.Relics.Event.Doll;

public class TianSuLuoDoll : CuteSakikoEventRelic, IModRightClickableRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
        }
    }

    public bool CanHandleRightClickLocal(ModRightClickContext context) => true;

    public Task OnRightClick(ModRightClickExecutionContext context)
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var fullPath = Path.Combine(dir, "audio", "ohyeah.mp3");
        AudioManager.PlaySound(fullPath);
        return Task.CompletedTask;
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
        await CreatureCmd.GainBlock(Owner.Creature, 3m, ValueProp.Move, null);
    }
}