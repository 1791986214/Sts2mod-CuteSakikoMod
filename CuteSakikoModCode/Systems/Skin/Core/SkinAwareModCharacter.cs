using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Characters;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

public abstract class SkinAwareModCharacter<TCardPool, TRelicPool, TPotionPool>
    : ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool>
    where TCardPool : CardPoolModel
    where TRelicPool : RelicPoolModel
    where TPotionPool : PotionPoolModel
{
    protected CharacterSkinDefinition? CurrentSkin
    {
        get
        {
            var reg = CharacterSkinRegistry.ForCharacter(GetType());
            if (reg == null || reg.AllSkins.Count == 0) return null;

            var choice = SkinResolver.GetLocalChoice(GetType());
            var idx = Math.Clamp(choice.ArtSkinIndex, 0, reg.AllSkins.Count - 1);
            return reg.AllSkins[idx];
        }
    }

    public override CharacterAssetProfile AssetProfile
        => CurrentSkin is { } skin
            ? SkinAssetBuilder.Build(
                skin,
                CharacterSelectBgPath,
                CharacterSelectIconPath,
                CharacterSelectLockedIconPath,
                MapMarkerPath)
            : base.AssetProfile;

    public override int StartingHp
    {
        get
        {
            if (CurrentSkin is not { } skin || skin.RelicPresets.Count == 0) return 75;
            return skin.RelicPresets[ResolveRelicIndex(skin)].StartingHp;
        }
    }

    public override int StartingGold
    {
        get
        {
            if (CurrentSkin is not { } skin || skin.RelicPresets.Count == 0) return 99;
            return skin.RelicPresets[ResolveRelicIndex(skin)].StartingGold;
        }
    }

    // ── 以下四项由角色固定提供，不随皮肤切换 ──
    protected virtual string? CharacterSelectBgPath => null;
    protected virtual string? CharacterSelectIconPath => null;
    protected virtual string? CharacterSelectLockedIconPath => null;
    protected virtual string? MapMarkerPath => null;

    protected override IEnumerable<CardModel> LocalStartingDeck
    {
        get
        {
            if (CurrentSkin is not { } skin || skin.DeckPresets.Count == 0)
                yield break;

            var choice = SkinResolver.GetLocalChoice(GetType());
            var idx = Math.Clamp(choice.DeckPresetIndex, 0, skin.DeckPresets.Count - 1);

            foreach (var (cardType, count) in skin.DeckPresets[idx].Cards)
            {
                var model = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
                for (var i = 0; i < Math.Max(count, 0); i++)
                    yield return model;
            }
        }
    }

    protected override IEnumerable<RelicModel> LocalStartingRelics
    {
        get
        {
            if (CurrentSkin is not { } skin || skin.RelicPresets.Count == 0)
                yield break;

            var choice = SkinResolver.GetLocalChoice(GetType());
            var idx = Math.Clamp(choice.RelicPresetIndex, 0, skin.RelicPresets.Count - 1);

            foreach (var relicType in skin.RelicPresets[idx].RelicTypes)
                yield return ModelDb.GetById<RelicModel>(ModelDb.GetId(relicType));
        }
    }

    private int ResolveRelicIndex(CharacterSkinDefinition skin)
    {
        var choice = SkinResolver.GetLocalChoice(GetType());
        return Math.Clamp(choice.RelicPresetIndex, 0, skin.RelicPresets.Count - 1);
    }
}