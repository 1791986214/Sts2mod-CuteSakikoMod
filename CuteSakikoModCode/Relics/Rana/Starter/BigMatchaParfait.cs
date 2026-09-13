using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace CuteSakikoMod.CuteSakikoModCode.Relics.Rana.Starter;

public class BigMatchaParfait : MatchaParfait
{
    // 初始杯数由 AfterObtained 决定，不在这里写死
    protected override int GetInitialCharges() => 0;

    public BigMatchaParfait()
    {
        DrawAmount = 2;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(DrawAmount),
        new EnergyVar(EnergyGain)
    };

    public override async Task AfterObtained()
    {
        // 查找玩家身上已有的普通芭菲（排除自己）
        var oldParfait = Owner?.Relics.OfType<MatchaParfait>().FirstOrDefault(r => r != this);

        if (oldParfait != null)
        {
            // 继承旧杯数 + 额外 6 杯
            Charges = oldParfait.Charges + 6;
        }
        else
        {
            // 玩家原本没有芭菲时，直接给 12 杯
            Charges = 12;
        }

        await Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is RestSiteRoom) Charges += 8;
        return Task.CompletedTask;
    }
}