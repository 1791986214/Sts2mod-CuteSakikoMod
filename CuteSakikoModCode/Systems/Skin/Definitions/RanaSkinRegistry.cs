using CuteSakikoMod.CuteSakikoModCode.Cards.Rana.Basic;
using CuteSakikoMod.CuteSakikoModCode.Character.Mygo;
using CuteSakikoMod.CuteSakikoModCode.Relics.Rana.Starter;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Definitions;

public sealed class RanaSkinRegistry : ICharacterSkinRegistry
{
    public Type CharacterType => typeof(CuteRana);

    // ─────────────────────────────────────────────────────────
    // 共享预设（唯一来源）：该角色所有皮肤默认共用
    // ─────────────────────────────────────────────────────────
    private static readonly CharacterPresetSet SharedPresets = new(
        DeckPresets:
        [
            new DeckPreset("rana_default", "RANA_DECK_DEFAULT",
            [
                (typeof(RanaStrike), 4),
                (typeof(RanaDefend), 4),
                (typeof(EatParfait), 1),
                (typeof(WantLive),   1),
            ]),
        ],
        RelicPresets:
        [
            new RelicPreset("rana_starter", "RANA_RELIC_DEFAULT",
                [typeof(MatchaParfait)]),
        ]);

    // ─────────────────────────────────────────────────────────
    // 皮肤：只写差异化字段（HP / Gold / Assets）
    // ─────────────────────────────────────────────────────────
    public IReadOnlyList<CharacterSkinDefinition> AllSkins { get; } =
    [
        CharacterSkinDefinition.FromPresets(
            id: "rana",
            displayNameKey: "RANA_SKIN_DEFAULT",
            startingHp: 70,
            startingGold: 99,
            assets: new SkinAssets(
                VisualsPath:       "res://CuteSakikoMod/scenes/char/rana/rana.tscn",
                EnergyCounterPath: "res://CuteSakikoMod/scenes/char/anon/anon_energy_counter.tscn",
                MerchantPath:      "res://CuteSakikoMod/scenes/char/rana/rana_merchant.tscn",
                RestSitePath:      "res://CuteSakikoMod/scenes/char/rana/rana_rest_site.tscn",
                IconTexturePath:        "res://CuteSakikoMod/images/charui/rana/character_icon_rana.png",
                IconOutlineTexturePath: "res://CuteSakikoMod/images/charui/rana/character_icon_rana_outline.png",
                IconPath:               "res://CuteSakikoMod/scenes/char/rana/rana_icon.tscn",
                TrailPath:              "res://CuteSakikoMod/scenes/ui/card_trail_anon.tscn",
                ArmPointingPath: "res://CuteSakikoMod/images/charui/rana/multiplayer_hand_point.png",
                ArmRockPath:     "res://CuteSakikoMod/images/charui/rana/multiplayer_hand_rock.png",
                ArmPaperPath:    "res://CuteSakikoMod/images/charui/rana/multiplayer_hand_paper.png",
                ArmScissorsPath: "res://CuteSakikoMod/images/charui/rana/multiplayer_hand_scissors.png"),
            presets: SharedPresets),
        
        CharacterSkinDefinition.FromPresets(
            id: "tenday_rana",
            displayNameKey: "RANA_SKIN_TENDAY",
            startingHp: 70,
            startingGold: 99,
            assets: new SkinAssets(
                VisualsPath:       "res://CuteSakikoMod/scenes/skin/rana/tenday/tendayrana.tscn",
                EnergyCounterPath: "res://CuteSakikoMod/scenes/char/anon/anon_energy_counter.tscn",
                MerchantPath:      "res://CuteSakikoMod/scenes/skin/rana/tenday/tendayrana_merchant.tscn",
                RestSitePath:      "res://CuteSakikoMod/scenes/skin/rana/tenday/tendayrana_rest_site.tscn",
                IconTexturePath:        "res://CuteSakikoMod/images/skin/rana/tenday/character_icon_rana.png",
                IconOutlineTexturePath: "res://CuteSakikoMod/images/skin/rana/tenday/character_icon_rana_outline.png",
                IconPath:               "res://CuteSakikoMod/scenes/skin/rana/tenday/tendayrana_icon.tscn",
                TrailPath:              "res://CuteSakikoMod/scenes/ui/card_trail_anon.tscn",
                ArmPointingPath: "res://CuteSakikoMod/images/skin/rana/tenday/multiplayer_hand_point.png",
                ArmRockPath:     "res://CuteSakikoMod/images/skin/rana/tenday/multiplayer_hand_rock.png",
                ArmPaperPath:    "res://CuteSakikoMod/images/skin/rana/tenday/multiplayer_hand_paper.png",
                ArmScissorsPath: "res://CuteSakikoMod/images/skin/rana/tenday/multiplayer_hand_scissors.png"),
            presets: SharedPresets),
    ];
}