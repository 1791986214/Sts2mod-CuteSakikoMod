using CuteSakikoMod.CuteSakikoModCode.Monsters.Weak;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;

namespace CuteSakikoMod.CuteSakikoModCode.Encounters.Event;

public class LuoEncounter : ModEncounterTemplate
{
    public override bool IsValidForAct(ActModel act) => false;

    public override RoomType RoomType => RoomType.Monster;
    public override bool IsWeak => false;

    public override IReadOnlyList<string> Slots => new[]
    {
        "snail1", "snail2", "snail3",
        "snail4", "snail5", "snail6"
    };

    public override EncounterAssetProfile AssetProfile => new(
        EncounterScenePath: "res://CuteSakikoMod/scenes/encounter/luo_encounter.tscn"
    );

    public override float GetCameraScaling() => 0.9f;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => new MonsterModel[]
    {
        ModelDb.Monster<TianSuLuo>(),
        ModelDb.Monster<TianXiangLuo>(),
        ModelDb.Monster<Araluo>()
    };

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var pool = new List<MonsterModel>
        {
            ModelDb.Monster<TianSuLuo>(),
            ModelDb.Monster<TianXiangLuo>(),
            ModelDb.Monster<Araluo>()
        };

        var result = new List<(MonsterModel, string?)>();
        foreach (var slot in Slots)
        {
            var pick = pool[Rng.NextInt(pool.Count)];
            result.Add((pick.ToMutable(), slot));
        }

        return result;
    }
}