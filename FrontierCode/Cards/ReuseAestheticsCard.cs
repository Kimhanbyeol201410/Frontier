using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Frontier.Characters;

namespace Frontier.Cards;

// 재사용의 미학: 소멸 더미에서 1장을 손패로 가져온 뒤 이 카드는 소멸.
[Pool(typeof(ShumitCardPool))]
public sealed class ReuseAestheticsCard : ShumitCard
{
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
        if (exhaustCards.Count == 0)
        {
            await CardCmd.Exhaust(choiceContext, this);
            return;
        }

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

        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
