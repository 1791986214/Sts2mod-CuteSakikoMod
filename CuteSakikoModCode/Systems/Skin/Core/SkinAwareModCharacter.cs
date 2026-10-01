using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public abstract class SkinAwareModCharacter<TCardPool, TRelicPool, TPotionPool>
    : ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool>
    where TCardPool : CardPoolModel
    where TRelicPool : RelicPoolModel
    where TPotionPool : PotionPoolModel
{
    protected CharacterSkinDefinition? CurrentSkin
    {
        get
        {
            var reg = CharacterSkinRegistry.ForCharacter(GetType());
            if (reg == null || reg.AllSkins.Count == 0) return null;

            var choice = SkinResolver.GetLocalChoice(GetType());
            var idx = Math.Clamp(choice.ArtSkinIndex, 0, reg.AllSkins.Count - 1);
            return reg.AllSkins[idx];
        }
    }
    
    /// <summary>
    /// 非 Spine 皮肤的 per-player 动画状态机（AnimationPlayer 后端）。
    ///
    /// 调用链：
    ///   RitsuLib ModCreatureCombatAnimationPlaybackPatch
    ///     → NCreature.SetAnimationTrigger(trigger)
    ///     → StateMachineSlot.EnsureBuilt(entity.Player?.Character, monster, visuals)
    ///     → character.TryCreateCombatAnimationStateMachine(visuals)
    ///     → 本方法
    ///
    /// 反查 visualsRoot 上层 NCreature 拿到本局该玩家的 Player，
    /// 再按 netId 拿该玩家的皮肤。若皮肤标记走 AnimationPlayer 状态机，
    /// 用 Builder 声明状态图并构建；否则返回 null（走原版 Spine）。
    /// </summary>
    protected override ModAnimStateMachine? SetupCustomCombatAnimationStateMachine(
        Node visualsRoot, CharacterModel character)
    {
        // 1. 反查 Player
        var player = FindPlayerFromVisuals(visualsRoot);
        if (player == null)
        {
            GD.Print("[SkinAnim] player null, fallback to vanilla");
            return null;
        }

        // 2. 取该玩家的皮肤
        var charType = player.Character.GetType();
        if (!CharacterSkinRegistry.HasSkins(charType)) return null;

        var skin = SkinResolver.GetSkinForPlayer(player.NetId, charType);
        if (skin?.Assets.UsesAnimationPlayerStateMachine != true)
        {
            GD.Print($"[SkinAnim] skin={skin?.Id} not AnimationPlayer-based → vanilla");
            return null;
        }

        try
        {
            // 状态 id 必须 == AnimationPlayer 里的动画名（小写）
            // trigger 用原版 CreatureAnimator 派发的名称（大写）
            var builder = ModAnimStateMachineBuilder.Create()
                .AddState("idle_loop", loop: true).AsInitial().Done()
                .AddState("attack").WithNext("idle_loop").Done()
                .AddState("cast").WithNext("idle_loop").Done()
                .AddState("hurt").WithNext("idle_loop").Done()
                .AddState("die").Done();

            builder.AddAnyState("Idle",    "idle_loop");
            builder.AddAnyState("Relaxed", "idle_loop");
            builder.AddAnyState("Attack",  "attack");
            builder.AddAnyState("Cast",    "cast");
            builder.AddAnyState("PowerUp", "cast");
            builder.AddAnyState("Hit",     "hurt");
            builder.AddAnyState("Dead",    "die");
            builder.AddAnyState("Revive",  "idle_loop");

            // character: null —— 不用 canonical CharacterModel 的 VisualCues
            //   （canonical 是共享单例，saki/ob 的 cue 会互相覆盖）
            var sm = builder.BuildForVisualsRoot(visualsRoot, character: null);

            GD.Print($"[SkinAnim] Built state machine: skin={skin.Id} netId={player.NetId}");
            return sm;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[SkinAnim] Build failed: {ex}");
            return null;
        }
    }

    private static Player? FindPlayerFromVisuals(Node visualsRoot)
    {
        Node? n = visualsRoot;
        for (int i = 0; i < 5 && n != null; i++)
        {
            if (n is NCreature nc) return nc.Entity?.Player;
            n = n.GetParent();
        }
        return null;
    }

    public override CharacterAssetProfile AssetProfile
        => CurrentSkin is { } skin
            ? SkinAssetBuilder.Build(
                skin,
                CharacterSelectBgPath,
                CharacterSelectIconPath,
                CharacterSelectLockedIconPath,
                MapMarkerPath)
            : base.AssetProfile;

    public override int StartingHp
    {
        get
        {
            if (CurrentSkin is not { } skin || skin.RelicPresets.Count == 0) return 75;
            return skin.RelicPresets[ResolveRelicIndex(skin)].StartingHp;
        }
    }

    public override int StartingGold
    {
        get
        {
            if (CurrentSkin is not { } skin || skin.RelicPresets.Count == 0) return 99;
            return skin.RelicPresets[ResolveRelicIndex(skin)].StartingGold;
        }
    }

    // ── 以下四项由角色固定提供，不随皮肤切换 ──
    protected virtual string? CharacterSelectBgPath => null;
    protected virtual string? CharacterSelectIconPath => null;
    protected virtual string? CharacterSelectLockedIconPath => null;
    protected virtual string? MapMarkerPath => null;

    protected override IEnumerable<CardModel> LocalStartingDeck
    {
        get
        {
            if (CurrentSkin is not { } skin || skin.DeckPresets.Count == 0)
                yield break;

            var choice = SkinResolver.GetLocalChoice(GetType());
            var idx = Math.Clamp(choice.DeckPresetIndex, 0, skin.DeckPresets.Count - 1);

            foreach (var (cardType, count) in skin.DeckPresets[idx].Cards)
            {
                var model = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
                for (var i = 0; i < Math.Max(count, 0); i++)
                    yield return model;
            }
        }
    }

    protected override IEnumerable<RelicModel> LocalStartingRelics
    {
        get
        {
            if (CurrentSkin is not { } skin || skin.RelicPresets.Count == 0)
                yield break;

            var choice = SkinResolver.GetLocalChoice(GetType());
            var idx = Math.Clamp(choice.RelicPresetIndex, 0, skin.RelicPresets.Count - 1);

            foreach (var relicType in skin.RelicPresets[idx].RelicTypes)
                yield return ModelDb.GetById<RelicModel>(ModelDb.GetId(relicType));
        }
    }

    private int ResolveRelicIndex(CharacterSkinDefinition skin)
    {
        var choice = SkinResolver.GetLocalChoice(GetType());
        return Math.Clamp(choice.RelicPresetIndex, 0, skin.RelicPresets.Count - 1);
    }
}