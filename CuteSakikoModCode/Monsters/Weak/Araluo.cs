
using System.Reflection;
using CuteSakikoMod.CuteSakikoModCode.Systems;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;

namespace CuteSakikoMod.CuteSakikoModCode.Monsters.Weak;

[RegisterMonster]
public class Araluo : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 35, 30);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 45, 40);

    public override MonsterAssetProfile AssetProfile => new(
        "res://CuteSakikoMod/scenes/monster/araluo.tscn"
    );

    // 数值（高进阶变化）
    private int HeavyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int LightHitCount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    // 三个音效文件
    private static readonly string[] UmePowerFiles =
    {
        "umepower1.mp3",
        "umepower2.mp3",
        "umepower3.mp3"
    };

    private static readonly Random _rand = new();

    protected override NCreatureVisuals? TryCreateCreatureVisuals()
    {
        return RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var heavy = new MoveState("HEAVY_ATTACK", HeavyMove,
            new SingleAttackIntent(HeavyDamage));
        var buff = new MoveState("POWER_UP", BuffMove,
            new BuffIntent());
        var light = new MoveState("LIGHT_ATTACK", LightMove,
            new MultiAttackIntent(1, LightHitCount));

        // 循环：1 → 2 → 3 → 2 → 1 → 2 → 3 → 2 → ...
        // 即 [heavy, buff, light, buff] 循环
        heavy.FollowUpState = buff;
        buff.FollowUpState = light;
        light.FollowUpState = buff;
        buff.FollowUpState = heavy;   // 覆盖上面，让它以 heavy 为目标
        // 但这样会形成 heavy → buff → light → buff → heavy，正是我们想要的

        var states = new List<MonsterState> { heavy, buff, light };
        return new MonsterMoveStateMachine(states, heavy);
    }

    private async Task HeavyMove(IReadOnlyList<Creature> targets)
    {
        PlayRandomUmePower();

        await DamageCmd.Attack(HeavyDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.3f)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private async Task BuffMove(IReadOnlyList<Creature> targets)
    {
        PlayRandomUmePower();

        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
    }

    private async Task LightMove(IReadOnlyList<Creature> targets)
    {
        PlayRandomUmePower();

        await DamageCmd.Attack(1)
            .FromMonster(this)
            .WithHitCount(LightHitCount)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private static void PlayRandomUmePower()
    {
        string file = UmePowerFiles[_rand.Next(UmePowerFiles.Length)];
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var fullPath = Path.Combine(dir, "audio", file);
        AudioManager.PlaySound(fullPath);
    }
}