using MegaCrit.Sts2.Core.Models;
using CuteSakikoMod.CuteSakikoModCode.Systems.Skin.Core;

namespace CuteSakikoMod.CuteSakikoModCode.Character;

public abstract class CuteSakikoCharacter<TCardPool, TRelicPool, TPotionPool>
    : SkinAwareModCharacter<TCardPool, TRelicPool, TPotionPool>
    where TCardPool : CardPoolModel
    where TRelicPool : RelicPoolModel
    where TPotionPool : PotionPoolModel
{
    // 该模组所有角色的共享逻辑
}