namespace CuteSakikoMod.CuteSakikoModCode.Systems.Chord;

/// <summary>升级为 FlashAnonGuitar 时用于迁移的和弦数据。</summary>
public sealed class PendingChordMigration
{
    public string Bonus = "";
    public List<string> BonusChords = new();
    public string Chords = "";
    public string Temp = "";
}