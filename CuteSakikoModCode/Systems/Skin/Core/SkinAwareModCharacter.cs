using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
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
            // 只用于"没有 per-player patch 覆盖"的兜底场景
            // 具体玩家由 SkinCreatureCreatePatch 等 patch 分流
            var reg = CharacterSkinRegistry.ForCharacter(GetType());
            return reg?.AllSkins.Count > 0 ? reg.AllSkins[0] : null;
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
        => CurrentSkin?.StartingHp ?? 75;

    public override int StartingGold
        => CurrentSkin?.StartingGold ?? 99;

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
            var idx = System.Math.Clamp(choice.DeckPresetIndex, 0, skin.DeckPresets.Count - 1);

            foreach (var (cardType, count) in skin.DeckPresets[idx].Cards)
            {
                var model = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
                for (int i = 0; i < System.Math.Max(count, 0); i++)
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
            var idx = System.Math.Clamp(choice.RelicPresetIndex, 0, skin.RelicPresets.Count - 1);

            foreach (var relicType in skin.RelicPresets[idx].RelicTypes)
                yield return ModelDb.GetById<RelicModel>(ModelDb.GetId(relicType));
        }
    }
}