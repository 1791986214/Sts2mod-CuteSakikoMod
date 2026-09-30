
using CuteSakikoMod.CuteSakikoModCode.Others;
using CuteSakikoMod.CuteSakikoModCode.Relics.Anon.Starter;
using CuteSakikoMod.CuteSakikoModCode.Systems.Chord;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Keywords;

namespace CuteSakikoMod.CuteSakikoModCode.Cards.Anon.Rare;

public class ImpromptuPlay() : CuteAnonCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust, CutesakiKeywords.NoNote.GetModCardKeyword()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        TriggerBanter();

        var guitar = Owner.Relics.OfType<AnonGuitar>().FirstOrDefault();
        if (guitar == null) return;

        // 直接从 Deck 读取所有 Chord 卡的 ChordId
        var deck = PileType.Deck.GetPile(Owner);
        if (deck == null) return;

        var chordIds = deck.Cards
            .Where(c => c.Keywords != null &&
                        c.Keywords.Contains(CutesakiKeywords.Chord.GetModCardKeyword()))
            .Select(c => (c as CuteAnonCard)?.ChordId)
            .Where(id => !string.IsNullOrEmpty(id) && ChordManager.AllChords.ContainsKey(id))
            .Distinct()
            .ToList();

        if (chordIds.Count == 0) return;

        // 一次性演奏所有，共享首次加成和 ChordBonusPower
        await ChordNoteSystem.PlayChordsAsync(Owner, chordIds, choiceContext);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}