using STS2RitsuLib.Scaffolding.Characters;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public static class SkinAssetBuilder
{
    public static CharacterAssetProfile Build(
        CharacterSkinDefinition skin,
        string? characterSelectBgPath,
        string? characterSelectIconPath,
        string? characterSelectLockedIconPath,
        string? mapMarkerPath)
    {
        return CharacterAssetProfiles.Merge(
            CharacterAssetProfiles.Ironclad(),
            new CharacterAssetProfile(
                new CharacterSceneAssetSet(
                    skin.Assets.VisualsPath,
                    skin.Assets.EnergyCounterPath,
                    skin.Assets.MerchantPath,
                    skin.Assets.RestSitePath),
                new CharacterUiAssetSet(
                    skin.Assets.IconTexturePath,
                    skin.Assets.IconOutlineTexturePath,
                    skin.Assets.IconPath,
                    // 以下四项为角色固定资源，不随皮肤切换
                    characterSelectBgPath ?? skin.Assets.VisualsPath,
                    characterSelectIconPath ?? skin.Assets.VisualsPath,
                    characterSelectLockedIconPath ?? skin.Assets.VisualsPath,
                    MapMarkerPath: mapMarkerPath ?? skin.Assets.VisualsPath),
                new CharacterVfxAssetSet(skin.Assets.TrailPath),
                Multiplayer: new CharacterMultiplayerAssetSet(
                    skin.Assets.ArmPointingPath,
                    skin.Assets.ArmRockPath,
                    skin.Assets.ArmPaperPath,
                    skin.Assets.ArmScissorsPath)));
    }
}