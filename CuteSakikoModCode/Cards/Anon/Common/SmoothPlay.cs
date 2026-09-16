using CuteSakikoMod.CuteSakikoModCode.Relics.Anon.Starter;
using CuteSakikoMod.CuteSakikoModCode.Systems;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Common;

public class SmoothPlay : CuteAnonCard
{
    private bool _eventSubscribed;
    // 标记本回合是否被外部效果（如 Splash）设为免费
    private bool _externallyFreeThisTurn;

    public SmoothPlay() : base(4, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DamageVar(25m, ValueProp.Move); }
    }

    public override void AfterCreated()
    {
        base.AfterCreated();
        SubscribeAndRefresh();
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await base.AfterCardDrawn(choiceContext, card, fromHandDraw);
        if (card != this) return;

        SubscribeAndRefresh();

        // 检测外部免费效果：若当前费用已被其他效果降到0（音符此时还没生效），标记本回合跳过更新
        int currentCost = EnergyCost.GetWithModifiers(CostModifiers.Local);
        if (currentCost == 0)
        {
            _externallyFreeThisTurn = true;
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);
        if (Owner != null)
            UpdateCost();
    }

    private void SubscribeAndRefresh()
    {
        if (!_eventSubscribed)
        {
            ChordNoteSystem.PlayerNotesChanged += OnPlayerNotesChanged;
            _eventSubscribed = true;
        }
        UpdateCost();
    }

    private void OnPlayerNotesChanged(Player changedPlayer)
    {
        if (Owner == null || changedPlayer != Owner) return;
        UpdateCost();
    }

    private void UpdateCost()
    {
        if (Owner?.Creature?.CombatState == null) return;
        // 若被外部效果设为免费，本回合不做任何覆盖
        if (_externallyFreeThisTurn) return;

        var attackCount = ChordNoteSystem.GetCurrentNotes(Owner)
            .Count(n => n == CardType.Attack);

        int targetCost = Math.Max(0, 4 - attackCount);
        // 使用不带 reduceOnly 的 SetThisTurn，保证音符变化时费用能升降
        EnergyCost.SetThisTurn(targetCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        TriggerBanter();

        var combat = Owner.Creature.CombatState;
        if (combat == null) return;
        if (cardPlay.Target == null) return;

        // 清除所有音符
        ChordNoteSystem.ClearNotes(Owner);

        var damage = DynamicVars.Damage.IntValue;
        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        ChordNoteUIManager.UpdateNoteDisplay(Owner);

        // 打出后清除外部免费标记，避免影响下次抽到
        _externallyFreeThisTurn = false;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10m);
    }
}