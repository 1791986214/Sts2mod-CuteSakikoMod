namespace CuteSakikoMod.CuteSakikoModCode.Others.Config;

public sealed class RunGameplayConfigData
{
    public bool EggsCard { get; set; }
    public bool EnableModMonsters { get; set; } = true;
    public bool EnableCustomAncients { get; set; } = true;
    public bool EnableCustomEvents { get; set; } = true;
    public bool ScalePressureInMultiplayer { get; set; }

    public void CopyFrom(CuteSakikoModConfigData cfg)
    {
        EggsCard = cfg.EggsCard;
        EnableModMonsters = cfg.EnableModMonsters;
        EnableCustomAncients = cfg.EnableCustomAncients;
        EnableCustomEvents = cfg.EnableCustomEvents;
        ScalePressureInMultiplayer = cfg.ScalePressureInMultiplayer;
    }

    public void ApplyTo(CuteSakikoModConfigData cfg)
    {
        cfg.EggsCard = EggsCard;
        cfg.EnableModMonsters = EnableModMonsters;
        cfg.EnableCustomAncients = EnableCustomAncients;
        cfg.EnableCustomEvents = EnableCustomEvents;
        cfg.ScalePressureInMultiplayer = ScalePressureInMultiplayer;
    }
}