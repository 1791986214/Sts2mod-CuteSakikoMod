using CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Token;
using CuteSakikoMod.CuteSakikoModCode.Others;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
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

        for (var i = 0; i < 3; i++)
        {
            var options = new List<CardModel>
            {
                combatState.CreateCard<AtkNote>(Owner),
                combatState.CreateCard<SkillNote>(Owner),
                combatState.CreateCard<PowerNote>(Owner),
                combatState.CreateCard<SpeNote>(Owner)
            };

            var selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner);
            if (selected != null)
                await chords.OnNoteGenerated(choiceContext, Owner, selected.Type, triggerEffect: IsUpgraded);

            foreach (var option in options)
                combatState.RemoveCard(option);
        }
    }

    protected override void OnUpgrade()
    {
    }
}