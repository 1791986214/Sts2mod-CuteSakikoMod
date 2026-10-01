using CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Token;
using CuteSakikoMod.CuteSakikoModCode.Others;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;

namespace CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Uncommon;

public class Strum() : CuteAnonCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CutesakiKeywords.NoNote.GetModCardKeyword()];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get { yield return HoverTipFactory.FromKeyword(CutesakiKeywords.Noteify.GetModCardKeyword()); }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        TriggerBanter();
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var chords = Owner.GetChords();
        if (chords == null) return;

        // 4 张卡超 FromChooseACardScreen 上限，改用 FromSimpleGrid
        var prefs = new CardSelectorPrefs(
            new LocString("card_selection", "CUTE_SAKIKO_MOD_STRUM_SELECT"),
            1); // selectCount = 1

        for (var i = 0; i < 3; i++)
        {
            var options = new List<CardModel>
            {
                combatState.CreateCard<AtkNote>(Owner),
                combatState.CreateCard<SkillNote>(Owner),
                combatState.CreateCard<PowerNote>(Owner),
                combatState.CreateCard<SpeNote>(Owner)
            };

            var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, options, Owner, prefs);
            var picked = selected.FirstOrDefault();

            if (picked != null)
                await chords.OnNoteGenerated(choiceContext, Owner, picked.Type, triggerEffect: IsUpgraded);

            foreach (var option in options)
                combatState.RemoveCard(option);
        }
    }

    protected override void OnUpgrade()
    {
    }
}