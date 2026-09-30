using CuteSakikoMod.CuteSakikoModCode.Others;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;


namespace CuteSakikoMod.CuteSakikoModCode.Relics.Anon.Starter;

[RegisterTouchOfOrobasRefinement(typeof(FlashAnonGuitar))]
public class AnonGuitar : CuteAnonRelic, IChordProvider, IChordConfig, IChordDataStorage, IModRightClickableRelic
{
    // ==================== 序列化数据（游戏原生机制） ====================
    [SavedProperty]
    public string SavedChordsData { get; set; } = "";

    [SavedProperty]
    public string SavedBonusChordsData { get; set; } = "";

    [SavedProperty]
    public string SavedTemporaryChordsData { get; set; } = "";

    [SavedProperty]
    public string SavedLearnedChordsData { get; set; } = "";

    [SavedProperty]
    public string SavedChordCardBonusData { get; set; } = "";

    // ==================== 配置 ====================
    public override RelicRarity Rarity => RelicRarity.Starter;

    public int ChordPriority => 100;

    public virtual int FirstPlayBonus => 1;
    public virtual int BaseChordBonus => 0;
    public virtual int MaxLearnedChordsPerCategory => 1;

    protected override IEnumerable<string> RegisteredKeywordIds => [CutesakiKeywords.RememberChord];

    // ==================== 悬浮提示 ====================
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return new HoverTip(
                new LocString("static_hover_tips", "CUTE_SAKIKO_MOD_HOVER_TIP_INSTRUMENT.title"),
                new LocString("static_hover_tips", "CUTE_SAKIKO_MOD_HOVER_TIP_INSTRUMENT.description"));

            foreach (var tip in this.GetOrCreateChords().BuildHoverTips())
                yield return tip;
        }
    }

    // ==================== IChordProvider ====================
    public IReadOnlyList<string> GetAvailableChordIds(Player player)
        => this.GetOrCreateChords().GetAllEquippedChords();

    // ==================== 右键菜单 ====================
    public bool CanHandleRightClickLocal(ModRightClickContext context) => true;

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        var me = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState()?.Players);
        if (me == null || me.NetId != Owner.NetId) return;

        this.GetOrCreateChords().OpenManagementScreen(readOnly: true);
        await Task.CompletedTask;
    }

    // ==================== 生命周期转发 ====================
    public override async Task AfterObtained()
    {
        await base.AfterObtained();
        var chords = this.GetOrCreateChords();
        await chords.HandleAfterObtained();
    }

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();
        var chords = this.GetOrCreateChords();
        await chords.HandleBeforeCombatStart();
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var chords = this.Chords();
        if (chords == null) return;
        await chords.HandleAfterPlayerTurnStart(choiceContext, player);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var chords = this.Chords();
        if (chords == null) return;
        await chords.HandleAfterCardPlayed(choiceContext, cardPlay);
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        await base.AfterRoomEntered(room);
        var chords = this.Chords();
        if (chords == null) return;
        await chords.HandleAfterRoomEntered(room);
    }

    public override async Task AfterRemoved()
    {
        await base.AfterRemoved();
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var chords = this.Chords();
        if (chords != null)
            await chords.HandleAfterCombatEnd(room);

        await base.AfterCombatEnd(room);
    }
}