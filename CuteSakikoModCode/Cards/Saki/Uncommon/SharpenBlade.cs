using CuteSakikoMod.CuteSakikoModCode.CardPiles;
using CuteSakikoMod.CuteSakikoModCode.Cards.Saki.Token;
using CuteSakikoMod.CuteSakikoModCode.Others;
using CuteSakikoMod.CuteSakikoModCode.Powers.Basic;
using CuteSakikoMod.CuteSakikoModCode.Powers.Debuff;
using CuteSakikoMod.CuteSakikoModCode.Singletons;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace CuteSakikoMod.CuteSakikoModCode.Cards.Saki.Uncommon;

public class SharpenBlade() : CuteSakikoModCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CutesakiKeywords.Sword.GetModCardKeyword()];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard<KnightSword>(IsUpgraded);
            yield return HoverTipFactory.FromPower<PressurePower>();
            yield return HoverTipFactory.FromPower<BreakDownPower>();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        // 1. 增伤：仅在压力 > 0 时生效
        var pressure = Owner.Creature.GetPower<PressurePower>();
        var pressureAmount = pressure?.Amount ?? 0;
        if (pressureAmount > 0)
            SwordManager.IncreaseAllSwordDamage(pressureAmount, combat);

        // 2. 升级：无条件执行（与压力无关）
        if (IsUpgraded)
        {
            // 标准牌堆
            foreach (var pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust })
            {
                var pile = pileType.GetPile(Owner);
                if (pile == null) continue;
                foreach (var card in pile.Cards.ToList())
                    if (card is KnightSword ks && ks.IsUpgradable)
                    {
                        ks.UpgradeInternal();
                        ks.FinalizeUpgradeInternal();
                    }
            }

            // 遗忘堆（自定义 ModCardPile）
            var forgetPile = ForgetCardPile.Get(Owner);
            if (forgetPile != null)
                foreach (var card in forgetPile.Cards.ToList())
                    if (card is KnightSword ks && ks.IsUpgradable)
                    {
                        ks.UpgradeInternal();
                        ks.FinalizeUpgradeInternal();
                    }
        }
    }

    protected override void OnUpgrade()
    {
    }
}