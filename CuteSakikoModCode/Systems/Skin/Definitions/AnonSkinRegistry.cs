using CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Basic;
using CuteSakikoMod.CuteSakikoModCode.Character.Mygo;
using CuteSakikoMod.CuteSakikoModCode.Relics.Anon.Starter;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Definitions;

public sealed class AnonSkinRegistry : ICharacterSkinRegistry
{
    public Type CharacterType => typeof(CuteAnon);

    // ─────────────────────────────────────────────────────────
    // 共享预设（唯一来源）：该角色所有皮肤默认共用
    // ─────────────────────────────────────────────────────────
    private static readonly CharacterPresetSet SharedPresets = new(
        DeckPresets:
        [
            new DeckPreset("anon_default", "ANON_DECK_DEFAULT",
            [
                (typeof(AnonStrike), 4),
                (typeof(AnonDefend), 4),
                (typeof(PlayChord),  1),
            ]),
        ],
        RelicPresets:
        [
            new RelicPreset("anon_starter", "ANON_RELIC_DEFAULT",
                [typeof(AnonGuitar)]),
        ]);

    // ─────────────────────────────────────────────────────────
    // 皮肤：只写差异化字段（HP / Gold / Assets）
    // ─────────────────────────────────────────────────────────
    public IReadOnlyList<CharacterSkinDefinition> AllSkins { get; } =
    [
        CharacterSkinDefinition.FromPresets(
            id: "anon",
            displayNameKey: "ANON_SKIN_DEFAULT",
            startingHp: 70,
            startingGold: 99,
            assets: new SkinAssets(
                VisualsPath:       "res://CuteSakikoMod/scenes/char/anon/anon.tscn",
                EnergyCounterPath: "res://CuteSakikoMod/scenes/char/anon/anon_energy_counter.tscn",
                MerchantPath:      "res://CuteSakikoMod/scenes/char/anon/anon_merchant.tscn",
                RestSitePath:      "res://CuteSakikoMod/scenes/char/anon/anon_rest_site.tscn",
                IconTexturePath:        "res://CuteSakikoMod/images/charui/anon/character_icon_anon.png",
                IconOutlineTexturePath: "res://CuteSakikoMod/images/charui/anon/character_icon_anon_outline.png",
                IconPath:               "res://CuteSakikoMod/scenes/char/anon/anon_icon.tscn",
                TrailPath:              "res://CuteSakikoMod/scenes/ui/card_trail_anon.tscn",
                ArmPointingPath: "res://CuteSakikoMod/images/charui/saki/multiplayer_hand_point.png",
                ArmRockPath:     "res://CuteSakikoMod/images/charui/saki/multiplayer_hand_rock.png",
                ArmPaperPath:    "res://CuteSakikoMod/images/charui/saki/multiplayer_hand_paper.png",
                ArmScissorsPath: "res://CuteSakikoMod/images/charui/saki/multiplayer_hand_scissors.png"),
            presets: SharedPresets),
        
        CharacterSkinDefinition.FromPresets(
            id: "tenday_anon",
            displayNameKey: "ANON_SKIN_TENDAY",
            startingHp: 70,
            startingGold: 99,
            assets: new SkinAssets(
                VisualsPath:       "res://CuteSakikoMod/scenes/skin/anon/tenday/tendayanon.tscn",
                EnergyCounterPath: "res://CuteSakikoMod/scenes/char/anon/anon_energy_counter.tscn",
                MerchantPath:      "res://CuteSakikoMod/scenes/skin/anon/tenday/tendayanon_merchant.tscn",
                RestSitePath:      "res://CuteSakikoMod/scenes/skin/anon/tenday/tendayanon_rest_site.tscn",
                IconTexturePath:        "res://CuteSakikoMod/images/skin/anon/tenday/character_icon_anon.png",
                IconOutlineTexturePath: "res://CuteSakikoMod/images/skin/anon/tenday/character_icon_anon_outline.png",
                IconPath:               "res://CuteSakikoMod/scenes/skin/anon/tenday/tendayanon_icon.tscn",
                TrailPath:              "res://CuteSakikoMod/scenes/ui/card_trail_anon.tscn",
                ArmPointingPath: "res://CuteSakikoMod/images/skin/anon/tenday/multiplayer_hand_point.png",
                ArmRockPath:     "res://CuteSakikoMod/images/skin/anon/tenday/multiplayer_hand_rock.png",
                ArmPaperPath:    "res://CuteSakikoMod/images/skin/anon/tenday/multiplayer_hand_paper.png",
                ArmScissorsPath: "res://CuteSakikoMod/images/skin/anon/tenday/multiplayer_hand_scissors.png"),
            presets: SharedPresets),
    ];
}