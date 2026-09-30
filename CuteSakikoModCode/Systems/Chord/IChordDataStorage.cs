namespace CuteSakikoMod.CuteSakikoModCode.Systems.Chord;

/// <summary>
/// 和弦数据的序列化宿主。遗物实现它，ChordStorageCapability 通过它读写数据。
/// 每个属性都必须标 [SavedProperty]。
/// </summary>
public interface IChordDataStorage
{
    string SavedChordsData { get; set; }
    string SavedBonusChordsData { get; set; }
    string SavedTemporaryChordsData { get; set; }
    string SavedLearnedChordsData { get; set; }
    string SavedChordCardBonusData { get; set; }
}