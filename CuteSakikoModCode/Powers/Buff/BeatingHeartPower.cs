using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace CuteSakikoMod.CuteSakikoModCode.Powers.Buff;

public sealed class BeatingHeartPower : CuteSakikoModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 基类是 protected virtual IEnumerable<DynamicVar> CanonicalVars
    // 注册一个名为 "Cap" 的变量，供 powers.json 里的 {Cap} 使用
    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar("Cap", 0m)
    };

    protected override object? InitInternalData() => new Data();

    private class Data
    {
        public decimal DamageReceivedThisTurn;
    }

    /// <summary>上限 = 十日终焉当前层数 × 50。</summary>
    private decimal GetCap()
    {
        var endPower = Owner.GetPower<TenDayEndPower>();
        int days = endPower?.Amount ?? 0;
        return days * 50m;
    }

    /// <summary>
    /// 刷新 Cap 动态变量。
    /// 调用时机：① 首次挂上时（AfterApplied）② 每个敌方回合开始时（BeforeSideTurnStart）
    /// ③ 十日终焉层数变化时（MovementNotFinishedPower.AfterPowerAmountChanged 转发）。
    /// </summary>
    public void UpdateCap()
    {
        // 图鉴 / 预览时为 Canonical 实例，Owner 会抛 CanonicalModelException
        if (!IsMutable) return;
        DynamicVars["Cap"].BaseValue = GetCap();
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        UpdateCap();
        return Task.CompletedTask;
    }

    public override Decimal ModifyHpLostBeforeOstyLate(
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || amount <= 0) return amount;

        var data = GetInternalData<Data>();
        var cap = GetCap();
        var remaining = Math.Max(0, cap - data.DamageReceivedThisTurn);
        return Math.Min(amount, remaining);
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || result.WasFullyBlocked) return Task.CompletedTask;

        var data = GetInternalData<Data>();
        data.DamageReceivedThisTurn += result.UnblockedDamage;
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        // 每次敌方回合开始清零，保证上限是“每回合”
        GetInternalData<Data>().DamageReceivedThisTurn = 0;
        // 顺便刷新 Cap，保证悬浮提示显示最新值
        UpdateCap();
        return Task.CompletedTask;
    }
}