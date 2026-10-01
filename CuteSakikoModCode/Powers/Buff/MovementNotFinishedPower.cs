using CuteSakikoMod.CuteSakikoModCode.Monsters.Boss.TenDay;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace CuteSakikoMod.CuteSakikoModCode.Powers.Buff;

public sealed class MovementNotFinishedPower : CuteSakikoModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// SmartDescription 本身在基类是 public 非 virtual，不可重写；
    /// 但它内部的 key 来自 protected virtual string SmartDescriptionLocKey。
    /// 重写 key 即可让悬浮提示随天数变化。
    /// </summary>
    protected override string SmartDescriptionLocKey
    {
        get
        {
            // Canonical（图鉴 / 卡池预览）时不能碰 Owner，必须短路
            if (!IsMutable)
                return base.SmartDescriptionLocKey;

            var day = Owner.GetPower<TenDayEndPower>()?.Amount ?? 10;
            var key = $"{Id.Entry}.effect.{day}";

            // 词条缺失时回退到默认 smartDescription
            return LocString.Exists("powers", key) ? key : base.SmartDescriptionLocKey;
        }
    }

    private readonly HashSet<int> _triggeredDays = new();
    private bool _deathShieldAvailable;
    private bool _deathShieldUsed;

    // ───────── 伤害减免 ─────────
    private decimal GetDamageReduction()
    {
        var days = Owner.GetPower<TenDayEndPower>()?.Amount ?? 0;
        return days switch
        {
            <= 1 => 0.30m,
            2 => 0.25m,
            3 => 0.20m,
            4 => 0.15m,
            5 => 0.10m,
            6 => 0.05m,
            _ => 0m
        };
    }

    public override Decimal ModifyHpLostBeforeOstyLate(
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || amount <= 0) return amount;
        var reduction = GetDamageReduction();
        return amount * (1m - reduction);
    }

    // ───────── 一次性死亡防护 ─────────
    public override bool ShouldDie(Creature creature)
    {
        if (creature != Owner) return base.ShouldDie(creature);
        if (!_deathShieldAvailable || _deathShieldUsed) return base.ShouldDie(creature);
        return false;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner) return;
        if (!_deathShieldAvailable || _deathShieldUsed) return;

        _deathShieldUsed = true;
        var heal = Math.Max(1, (int)(creature.MaxHp * 0.5m));
        await CreatureCmd.Heal(creature, heal);
    }

    // ───────── 日数变化时触发 ─────────
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power is not TenDayEndPower endPower) return;
        if (endPower.Owner != Owner) return;

        // 十日终焉层数变化 → 跳动的心跳的 Cap 也要刷新
        Owner.GetPower<BeatingHeartPower>()?.UpdateCap();

        int day = endPower.Amount;
        if (!_triggeredDays.Add(day)) return;

        await TriggerDayEffect(choiceContext, day);
    }

    private async Task TriggerDayEffect(PlayerChoiceContext choiceContext, int day)
    {
        if (Owner.Monster is not TenDayTaki boss) return;

        switch (day)
        {
            case 10:
                // 无效果
                break;

            case 9:
                await PowerCmd.Apply<BeatingHeartPower>(
                    new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
                // AfterApplied 里已经刷过一次，这里再保险一次
                Owner.GetPower<BeatingHeartPower>()?.UpdateCap();
                break;

            case 8:
                _deathShieldAvailable = true;
                break;

            case 7:
                await boss.SummonAlly<TenDayAnon>(choiceContext, "snail1");
                await CreatureCmd.Heal(Owner, (int)(Owner.MaxHp * 0.10m));
                break;

            case 6:
                await boss.SummonAlly<TenDayRana>(choiceContext, "snail2");
                break;

            case 5:
                await boss.SummonAlly<TenDayTomori>(choiceContext, "snail3");
                break;

            case 4:
                await boss.SummonAlly<TenDayUmirin>(choiceContext, "snail4");
                break;

            case 3:
                await boss.SummonAlly<TenDaySoyo>(choiceContext, "snail5");
                break;

            case 2:
                await boss.SummonAlly<TenDaySaki>(choiceContext, "snail6");
                break;

            case 1:
                foreach (var player in Owner.CombatState.Players)
                {
                    await CreatureCmd.LoseMaxHp(
                        new ThrowingPlayerChoiceContext(), player.Creature, 10m, false);
                }
                break;
        }
    }
}