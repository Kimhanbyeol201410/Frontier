using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Cards;

// 융해: 손패에서 최대 ExhaustCount장을 소멸 + 열기 30 + 자신도 소멸.
// 강화마다 ExhaustCount가 1씩 증가.
[Pool(typeof(ShumitCardPool))]
public sealed class MeltCard : ShumitCard
{
    private const string ExhaustCountKey = "ExhaustCount";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar(ExhaustCountKey, 1m),
    };

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Exhaust };

    public MeltCard()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int exhaustCount = DynamicVars[ExhaustCountKey].IntValue;
        if (exhaustCount > 0)
        {
            CardSelectorPrefs prefs = new(CardSelectorPrefs.ExhaustSelectionPrompt, 0, exhaustCount);
            IEnumerable<CardModel> picked = await CardSelectCmd.FromHandForDiscard(
                choiceContext,
                Owner,
                prefs,
                (CardModel c) => !ReferenceEquals(c, this),
                this);
            foreach (CardModel victim in picked.ToList())
            {
                await CardCmd.Exhaust(choiceContext, victim);
            }
        }

        await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, 30m, this);
        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[ExhaustCountKey].UpgradeValueBy(1m);
    }
}
