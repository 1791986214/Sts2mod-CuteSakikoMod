namespace CuteSakikoMod.CuteSakikoModCode.Others.Config;

public sealed class GameplayConfigDto
{
    public bool EggsCard { get; set; }
    public bool EnableModMonsters { get; set; } = true;
    public bool EnableCustomAncients { get; set; } = true;
    public bool EnableCustomEvents { get; set; } = true;
    public bool ScalePressureInMultiplayer { get; set; }

    public static GameplayConfigDto FromConfig(CuteSakikoModConfigData cfg)
    {
        return new GameplayConfigDto
        {
            EggsCard = cfg.EggsCard,
            EnableModMonsters = cfg.EnableModMonsters,
            EnableCustomAncients = cfg.EnableCustomAncients,
            EnableCustomEvents = cfg.EnableCustomEvents,
            ScalePressureInMultiplayer = cfg.ScalePressureInMultiplayer
        };
    }

    public void ApplyTo(CuteSakikoModConfigData cfg)
    {
        cfg.EggsCard = EggsCard;
        cfg.EnableModMonsters = EnableModMonsters;
        cfg.EnableCustomAncients = EnableCustomAncients;
        cfg.EnableCustomEvents = EnableCustomEvents;
        cfg.ScalePressureInMultiplayer = ScalePressureInMultiplayer;
    }

    public GameplayConfigDto Clone()
    {
        return new GameplayConfigDto
        {
            EggsCard = EggsCard,
            EnableModMonsters = EnableModMonsters,
            EnableCustomAncients = EnableCustomAncients,
            EnableCustomEvents = EnableCustomEvents,
            ScalePressureInMultiplayer = ScalePressureInMultiplayer
        };
    }
}

public sealed class GameplayConfigDelta
{
    public bool? EggsCard { get; set; }
    public bool? EnableModMonsters { get; set; }
    public bool? EnableCustomAncients { get; set; }
    public bool? EnableCustomEvents { get; set; }
    public bool? ScalePressureInMultiplayer { get; set; }
}