using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Cards;

// 재사용의 미학:
//   0강: 1코스트, 소멸 더미에서 1장을 손패로, 사용 후 소멸.
//   1강(재련 1): 0코스트로 강화 (사용 후 소멸 유지).
//   2강(재련 2): 사용 시 [소멸] 대신 [덱에서 영구 제거]. 키워드 영역에서 [소멸]도 제거.
[Pool(typeof(ShumitCardPool))]
public sealed class ReuseAestheticsCard : ShumitCard
{
    private const string RemoveOnPlayKey = "RemoveOnPlay";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar(RemoveOnPlayKey, 0m),
    };

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override bool IsPlayable =>
        base.IsPlayable && PileType.Exhaust.GetPile(Owner).Cards.Count > 0;

    public ReuseAestheticsCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<CardModel> exhaustCards = PileType.Exhaust.GetPile(Owner).Cards;
        if (exhaustCards.Count > 0)
        {
            CardSelectorPrefs prefs = new(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
            IEnumerable<CardModel> picked = await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                exhaustCards.ToList(),
                Owner,
                prefs);
            CardModel? card = picked.FirstOrDefault();
            if (card != null)
            {
                await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom, this);
            }
        }

        if (CurrentUpgradeLevel >= 2)
        {
            // 영구 제거: 전투 인스턴스는 전투 더미에서 빼내고, 덱 원본은 SwipePower 패턴대로 RemoveFromDeck 처리.
            CardModel? deckVersion = DeckVersion;
            await CardPileCmd.RemoveFromCombat(this);
            if (deckVersion != null && deckVersion.Pile != null && deckVersion.Pile.Type == PileType.Deck)
            {
                await CardPileCmd.RemoveFromDeck(deckVersion, showPreview: false);
            }
        }
        else
        {
            await CardCmd.Exhaust(choiceContext, this);
        }
    }

    protected override void OnUpgrade()
    {
        if (CurrentUpgradeLevel == 1)
        {
            EnergyCost.UpgradeBy(-1);
        }
        else if (CurrentUpgradeLevel >= 2)
        {
            RemoveKeyword(CardKeyword.Exhaust);
            DynamicVars[RemoveOnPlayKey].BaseValue = 1m;
        }
    }
}
