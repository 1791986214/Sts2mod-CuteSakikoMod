using CuteSakikoMod.CuteSakikoModCode.Relics.Anon;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace CuteSakikoMod.CuteSakikoModCode.Relics.Anon.Starter;

public class FlashAnonGuitar : AnonGuitar
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override int FirstPlayBonus => 3;
    public override int MaxLearnedChordsPerCategory => 2;

    public override async Task AfterObtained()
    {
        var mine = this.GetOrCreateChords();

        if (Owner != null && ChordStorageCapability.TryGetPendingMigration(Owner, out var data))
        {
            mine.RestoreChordData(data.Chords, data.Bonus, data.Temp);
            ChordStorageCapability.RemovePendingMigration(Owner);
        }
        else if (Owner != null)
        {
            var oldGuitar = Owner.Relics.OfType<AnonGuitar>()
                .FirstOrDefault(r => r is not FlashAnonGuitar && r != this);
            if (oldGuitar != null)
                oldGuitar.GetOrCreateChords().CopyTo(mine);
        }

        await base.AfterObtained();

        foreach (var cat in new[] { ChordCategory.Major, ChordCategory.Minor, ChordCategory.Dominant })
            mine.FillCategorySlots(cat);
    }
}