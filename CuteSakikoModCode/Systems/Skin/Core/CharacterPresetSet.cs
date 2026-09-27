using System.Collections.Generic;

namespace CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

/// <summary>
/// 某个角色共享的「牌组预设 + 遗物预设」集合。
/// 同一角色的多个皮肤通常共用同一套；如需差异化，
/// 用 CharacterSkinDefinition.FromPresets(..., DeckPresetsOverride: ...) 覆盖。
/// </summary>
public sealed record CharacterPresetSet(
    IReadOnlyList<DeckPreset> DeckPresets,
    IReadOnlyList<RelicPreset> RelicPresets);