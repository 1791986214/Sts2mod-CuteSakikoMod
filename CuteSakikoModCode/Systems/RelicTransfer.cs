using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace CuteSakikoMod.CuteSakikoModCode.Systems;

public static class RelicTransfer
{
    // 把同一个遗物实例从一个玩家转到另一个玩家，保留其内部状态（会触发 AfterObtained）
    public static async Task Transfer(RelicModel relic, Player target)
    {
        if (relic.Owner == target) return;
        await RelicCmd.Remove(relic);
        ClearOwner(relic);
        await RelicCmd.Obtain(relic, target);
    }

    // 只移除 + 清 owner，不立即给新玩家。
    public static async Task RemoveAndDetach(RelicModel relic)
    {
        await RelicCmd.Remove(relic);
        ClearOwner(relic);
    }

    // ★ 交换专用：添加到玩家但不触发 AfterObtained，避免拾取效果重复触发
    public static Task AttachWithoutObtained(RelicModel relic, Player target)
    {
        relic.AssertMutable();
        var runState = target.RunState;

        // 与 RelicCmd.Obtain 相同的注册流程，仅去掉最后的 AfterObtained()
        runState.CurrentMapPointHistoryEntry?
            .GetEntry(target.NetId).RelicChoices
            .Add(new ModelChoiceHistoryEntry(relic.Id, true));

        target.AddRelicInternal(relic, -1);

        if (!relic.IsStackable)
        {
            target.RelicGrabBag.Remove(relic);
            runState.SharedRelicGrabBag.Remove(relic);
        }

        if (LocalContext.IsMe(target))
        {
            NRun.Instance?.GlobalUi.RelicInventory.AnimateRelic(relic);
            NDebugAudioManager.Instance?.Play("relic_get.mp3");
            SaveManager.Instance.MarkRelicAsSeen(relic);
        }

        relic.FloorAddedToDeck = runState.TotalFloor;

        return Task.CompletedTask;
    }

    private static void ClearOwner(RelicModel relic)
    {
        var field = typeof(RelicModel).GetField("_owner",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(relic, null);
    }
}