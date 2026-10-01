namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public sealed record SkinAssets(
    string VisualsPath,
    string EnergyCounterPath,
    string MerchantPath,
    string RestSitePath,
    string IconTexturePath,
    string IconOutlineTexturePath,
    string IconPath,
    string TrailPath,
    string ArmPointingPath,
    string ArmRockPath,
    string ArmPaperPath,
    string ArmScissorsPath,
    // ★ 新增：true → 该皮肤走 AnimationPlayer 状态机
    //          false（默认）→ 走原版 Spine 路径（saki 皮肤）
    bool UsesAnimationPlayerStateMachine = true);