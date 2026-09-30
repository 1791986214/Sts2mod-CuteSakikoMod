using CuteSakikoMod.CuteSakikoModCode.Others;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Keywords;

namespace CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Basic;

public class TurnIntoNote() : CuteAnonCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    // 特殊演奏（本身不产生音符）+ 化为音符（供悬浮提示使用）
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CutesakiKeywords.NoNote.GetModCardKeyword()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromKeyword(CutesakiKeywords.Noteify.GetModCardKeyword()); }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        TriggerBanter();

        var hand = PileType.Hand.GetPile(Owner);
        if (hand == null) return;

        var candidates = hand.Cards.Where(c => c != this).ToList();
        if (candidates.Count == 0) return;

        int selectCount = Math.Min(DynamicVars.Cards.IntValue, candidates.Count);
        if (selectCount <= 0) return;

        var prefs = new CardSelectorPrefs(
            new LocString("cards", "CUTE_SAKIKO_MOD_CARD_TURN_INTO_NOTE.selectionScreenPrompt"),
            selectCount,
            selectCount);

        var selected = await CardSelectCmd.FromHand(
            choiceContext, Owner, prefs, c => c != this, this);

        var selectedList = selected.ToList();
        if (selectedList.Count == 0) return;

        // 先记录类型（消耗后 Pile 会变，但 Type 仍可读；不过提前记录更稳）
        var noteTypes = selectedList.Select(c => c.Type).ToList();

        // 消耗
        foreach (var card in selectedList)
            await CardCmd.Exhaust(choiceContext, card);

        // 依次：添加音符（触发和弦匹配）+ 触发对应效果
        foreach (var noteType in noteTypes)
        {
            await ChordNoteSystem.AddNoteAsync(Owner, noteType, choiceContext, triggerEffect: true);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1); // 1 → 2
    }
}