using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace CuteSakikoMod.CuteSakikoModCode.Relics.Anon.Common;

public class AnonMusicScore : CuteAnonRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        var chords = Owner.GetChords();
        if (chords == null) return;

        ChordCmd.AddRandomBonusChord(chords);
    }
}