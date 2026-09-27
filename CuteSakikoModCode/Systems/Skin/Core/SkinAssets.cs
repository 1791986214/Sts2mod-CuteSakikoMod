namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

/// <summary>
/// 一个皮肤的全部资源路径。
/// 角色选择背景、角色选择图标、地图标记图标由角色固定提供，不属于皮肤。
/// </summary>
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
    string ArmScissorsPath);