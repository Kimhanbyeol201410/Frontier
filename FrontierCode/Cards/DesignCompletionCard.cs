using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Frontier.Characters;

namespace Frontier.Cards;

// 설계의 완성
[Pool(typeof(ShumitCardPool))]
public sealed class DesignCompletionCard : ShumitCard
{
    private const string PickCountKey = "PickCount";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(PickCountKey, 1m) };

    public DesignCompletionCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override void OnUpgrade()
    {
        DynamicVars[PickCountKey].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<CardModel> drawCards = PileType.Draw.GetPile(Owner).Cards;
        if (drawCards.Count == 0)
        {
            await CardCmd.Exhaust(choiceContext, this);
            return;
        }

        int pickCount = System.Math.Min(DynamicVars[PickCountKey].IntValue, drawCards.Count);
        CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, pickCount);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromSimpleGrid(choiceContext, drawCards.ToList(), Owner, prefs);
        foreach (CardModel c in picked)
        {
            CardCmd.Upgrade(c, CardPreviewStyle.HorizontalLayout);
            await CardPileCmd.Add(c, PileType.Hand, CardPilePosition.Bottom, this);
        }

        await CardCmd.Exhaust(choiceContext, this);
    }
}
