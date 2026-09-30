using CuteSakikoMod.CuteSakikoModCode.Others;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Keywords;

namespace CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Common;

public class SlowerBeat() : CuteAnonCard(2, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new BlockVar(7m, ValueProp.Move);
            yield return new DynamicVar("BlockNextTurn", 5m);
            yield return new DynamicVar("notes", 1);
        }
    }
    
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromKeyword(CutesakiKeywords.Noteify.GetModCardKeyword()); }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        TriggerBanter();

        var block = DynamicVars.Block.IntValue;
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);

        var nextTurnBlock = (int)DynamicVars["BlockNextTurn"].BaseValue;
        await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, Owner.Creature, nextTurnBlock, Owner.Creature, this);

        ChordNoteSystem.Activate(Owner);

        var manualNoteCount = DynamicVars["notes"].IntValue;
        for (var i = 0; i < manualNoteCount; i++){
            await ChordNoteSystem.AddNoteAsync(Owner, CardType.Skill, choiceContext,true);
        }

        ChordNoteUIManager.UpdateNoteDisplay(Owner);
        ChordNoteUIManager.UpdateStoredChordDisplay(Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["notes"].UpgradeValueBy(1);
        DynamicVars["BlockNextTurn"].UpgradeValueBy(2m);
    }
}