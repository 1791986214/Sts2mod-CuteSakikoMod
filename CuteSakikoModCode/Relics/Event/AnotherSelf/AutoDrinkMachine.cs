
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Interactions.RightClick;

namespace CuteSakikoMod.CuteSakikoModCode.Relics.Event.AnotherSelf;

public class AutoDrinkMachine : CuteSakikoEventRelic,
    IModRightClickableRelic,
    IRelicExtraIconAmountLabelSpecsProvider,
    IRelicExtraIconAmountLabelsChangeSource
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    private const int GoldCost = 15;
    private const int SlotThreshold = 75;

    private int _accumulatedSpent;

    /// <summary>
    /// 累计已消费但尚未兑换栏位的金币数，跨战斗保存。
    /// 注意：[SavedProperty] 只能标注属性，不能标注字段。
    /// </summary>
    [SavedProperty]
    private int AccumulatedSpent
    {
        get => _accumulatedSpent;
        set
        {
            AssertMutable();
            _accumulatedSpent = value;
            InvalidateLabels(); // 数值变化 → 刷新角标
        }
    }

    // —— 角标接口 ——

    public event Action? RelicExtraIconAmountLabelsInvalidated;

    public IReadOnlyList<ExtraIconAmountLabelSpec> GetRelicExtraIconAmountLabelSpecs()
    {
        return
        [
            ExtraIconAmountLabelSpec.Plain(
                ExtraIconAmountLabelCorner.BottomRight,
                $"{AccumulatedSpent}/{SlotThreshold}")
        ];
    }

    private void InvalidateLabels()
    {
        RelicExtraIconAmountLabelsInvalidated?.Invoke();
        InvokeDisplayAmountChanged();
    }

    // —— 右键交互 ——

    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        var player = context.Player;
        return player is { Gold: >= GoldCost, HasOpenPotionSlots: true };
    }

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        var player = context.Player;
        if (player.Gold < GoldCost || !player.HasOpenPotionSlots)
            return;

        await PlayerCmd.LoseGold(GoldCost, player, GoldLossType.Spent);

        AccumulatedSpent += GoldCost;

        if (AccumulatedSpent >= SlotThreshold)
        {
            AccumulatedSpent -= SlotThreshold;
            await PlayerCmd.GainMaxPotionCount(1, player);
            Flash();
        }

        var randomPotion = PotionFactory.CreateRandomPotionInCombat(
            player,
            player.RunState.Rng.CombatPotionGeneration
        ).ToMutable();
        await PotionCmd.TryToProcure(randomPotion, player);
    }
}