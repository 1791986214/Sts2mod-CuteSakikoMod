using CuteSakikoMod.CuteSakikoModCode.Cards.Saki.Basic;
using CuteSakikoMod.CuteSakikoModCode.Character.Mujica;
using CuteSakikoMod.CuteSakikoModCode.Relics.Saki.Oblivionis;
using CuteSakikoMod.CuteSakikoModCode.Relics.Saki.Starter;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Definitions;

public sealed class SakiSkinRegistry : ICharacterSkinRegistry
{
    public Type CharacterType => typeof(CuteSaki);

    // ─────────────────────────────────────────────────────────
    // 共享预设（唯一来源）：同一个角色的所有皮肤默认共用
    // ─────────────────────────────────────────────────────────
    private static readonly CharacterPresetSet SharedPresets = new(
        DeckPresets:
        [
            new DeckPreset("saki_default", "SAKI_DECK_DEFAULT",
            [
                (typeof(PianoStrike), 4),
                (typeof(DefendSaki),  4),
                (typeof(GoWork),      1),
            ]),
            new DeckPreset("ob_default", "SAKI_DECK_OB",
            [
                (typeof(PianoStrike), 4),
                (typeof(DefendSaki),  4),
                (typeof(TemporaryForgetfulness), 1),
            ]),
        ],
        RelicPresets:
        [
            new RelicPreset("saki_starter", "SAKI_RELIC_DEFAULT",80,88, [typeof(KabutoNote)]),
            new RelicPreset("ob_starter",   "SAKI_RELIC_OB", 70,111,   [typeof(ObMask)]),
        ]);

    // ─────────────────────────────────────────────────────────
    // 皮肤：只写差异化字段（HP / Gold / Assets）
    // ─────────────────────────────────────────────────────────
    public IReadOnlyList<CharacterSkinDefinition> AllSkins { get; } =
    [
        CharacterSkinDefinition.FromPresets(
            id: "saki",
            displayNameKey: "SAKI_SKIN_DEFAULT",
            assets: new SkinAssets(
                VisualsPath:       "res://CuteSakikoMod/scenes/char/saki/sakiko.tscn",
                EnergyCounterPath: "res://CuteSakikoMod/scenes/char/saki/saki_energy_counter.tscn",
                MerchantPath:      "res://CuteSakikoMod/scenes/char/saki/saki_merchant.tscn",
                RestSitePath:      "res://CuteSakikoMod/scenes/char/saki/saki_rest_site.tscn",
                IconTexturePath:        "res://CuteSakikoMod/images/charui/saki/character_icon_saki.png",
                IconOutlineTexturePath: "res://CuteSakikoMod/images/charui/saki/character_icon_saki_outline.png",
                IconPath:               "res://CuteSakikoMod/scenes/char/saki/saki_icon.tscn",
                TrailPath:              "res://CuteSakikoMod/scenes/ui/card_trail_sakiko.tscn",
                ArmPointingPath: "res://CuteSakikoMod/images/charui/saki/multiplayer_hand_point.png",
                ArmRockPath:     "res://CuteSakikoMod/images/charui/saki/multiplayer_hand_rock.png",
                ArmPaperPath:    "res://CuteSakikoMod/images/charui/saki/multiplayer_hand_paper.png",
                ArmScissorsPath: "res://CuteSakikoMod/images/charui/saki/multiplayer_hand_scissors.png"),
            presets: SharedPresets),

        CharacterSkinDefinition.FromPresets(
            id: "ob",
            displayNameKey: "SAKI_SKIN_OB",
            assets: new SkinAssets(
                VisualsPath:       "res://CuteSakikoMod/scenes/skin/saki/ob/ob.tscn",
                EnergyCounterPath: "res://CuteSakikoMod/scenes/char/saki/saki_energy_counter.tscn",
                MerchantPath:      "res://CuteSakikoMod/scenes/skin/saki/ob/ob_merchant.tscn",
                RestSitePath:      "res://CuteSakikoMod/scenes/skin/saki/ob/ob_rest_site.tscn",
                IconTexturePath:        "res://CuteSakikoMod/images/skin/saki/ob/character_icon_ob.png",
                IconOutlineTexturePath: "res://CuteSakikoMod/images/skin/saki/ob/character_icon_ob_outline.png",
                IconPath:               "res://CuteSakikoMod/scenes/skin/saki/ob/ob_icon.tscn",
                TrailPath:              "res://CuteSakikoMod/scenes/ui/card_trail_sakiko.tscn",
                ArmPointingPath: "res://CuteSakikoMod/images/skin/saki/ob/multiplayer_hand_point.png",
                ArmRockPath:     "res://CuteSakikoMod/images/skin/saki/ob/multiplayer_hand_rock.png",
                ArmPaperPath:    "res://CuteSakikoMod/images/skin/saki/ob/multiplayer_hand_paper.png",
                ArmScissorsPath: "res://CuteSakikoMod/images/skin/saki/ob/multiplayer_hand_scissors.png"),
            presets: SharedPresets),
    ];
}