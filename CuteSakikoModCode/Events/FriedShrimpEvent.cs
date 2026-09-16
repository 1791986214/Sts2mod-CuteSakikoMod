
using CuteSakikoMod.CuteSakikoModCode.Encounters.Event;
using CuteSakikoMod.CuteSakikoModCode.Relics.Event.Doll;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace CuteSakikoMod.CuteSakikoModCode.Events;

[RegisterSharedEvent]
public sealed class FriedShrimpEvent : CuteSakikoEvent
{
    private int _combatContext;
    private IHoverTip[]? _dollHoverTips;

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://CuteSakikoMod/images/events/fried_shrimp.png"
    );

    public override bool IsShared => true;

    
    protected override bool IsAllowedInternal(IRunState runState) => true;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        // 缓存三个玩偶遗物的完整提示（含遗物自身描述 + 额外提示）
        _dollHoverTips ??= HoverTipFactory.FromRelic<TianSuLuoDoll>()
            .Concat(HoverTipFactory.FromRelic<TianXiangLuoDoll>())
            .Concat(HoverTipFactory.FromRelic<AraluoDoll>())
            .ToArray();

        return new List<EventOption>
        {
            new(this, CrossBush, InitialOptionKey("CROSS_BUSH"), _dollHoverTips),
            new(this, Detour, InitialOptionKey("DETOUR")),
            new(this, RestHere, InitialOptionKey("REST_HERE"))
        };
    }

    private Task CrossBush()
    {
        _combatContext = 1;
        EnterCombatWithoutExitingEvent<LuoEncounter>(
            Array.Empty<Reward>(),
            shouldResumeAfterCombat: true
        );
        return Task.CompletedTask;
    }

    private async Task Detour()
    {
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner!.Creature,
            8m,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null
        );

        var selected = await CardSelectCmd.FromDeckForRemoval(
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1)
        );
        var card = selected.FirstOrDefault();
        if (card != null)
        {
            await CardPileCmd.RemoveFromDeck(card);
        }

        SetEventFinished(PageDescription("DETOUR_DESC"));
    }

    private async Task RestHere()
    {
        var healAmount = Owner!.Creature.MaxHp * 0.3m;
        await CreatureCmd.Heal(Owner.Creature, healAmount);

        _combatContext = 2;
        EnterCombatWithoutExitingEvent<LuoEncounter>(
            Array.Empty<Reward>(),
            shouldResumeAfterCombat: true
        );
    }

    public override async Task Resume(AbstractRoom room)
    {
        if (_combatContext == 1)
        {
            await RewardsCmd.OfferCustom(Owner!, new List<Reward>
            {
                new RelicReward(ModelDb.Relic<TianSuLuoDoll>().ToMutable(), Owner!),
                new RelicReward(ModelDb.Relic<TianXiangLuoDoll>().ToMutable(), Owner!),
                new RelicReward(ModelDb.Relic<AraluoDoll>().ToMutable(), Owner!)
            });

            SetEventFinished(PageDescription("CROSS_BUSH_WIN"));
        }
        else if (_combatContext == 2)
        {
            SetEventFinished(PageDescription("REST_HERE_WIN"));
        }
    }

    private LocString PageDescription(string pageKey) => L10NLookup($"{Id.Entry}.pages.{pageKey}.description");
}