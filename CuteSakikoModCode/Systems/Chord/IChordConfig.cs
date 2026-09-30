namespace CuteSakikoMod.CuteSakikoModCode.Systems.Chord;

/// <summary>
/// 和弦配置视图。遗物实现它，ChordStorageCapability 就能读取数值参数。
/// 不实现则用默认值（0 / 0 / 1）。
/// </summary>
public interface IChordConfig
{
    int FirstPlayBonus => 0;
    int BaseChordBonus => 0;
    int MaxLearnedChordsPerCategory => 1;
    int ChordPriority => 0;   // 新增
}