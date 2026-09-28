using MegaCrit.Sts2.Core.Models;

namespace CuteSakikoMod.CuteSakikoModCode.Powers.Buff;

/// <summary>
///     本回合的和弦增幅：包装 ChordBonusPower，回合结束时自动移除。
/// </summary>
public class ChordBonusThisTurnPower : CuteSakikoTemporaryPower
{
    public override PowerModel InternallyAppliedPower => ModelDb.Power<ChordBonusPower>();
}