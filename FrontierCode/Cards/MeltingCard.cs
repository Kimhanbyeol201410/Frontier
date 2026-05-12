using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Frontier.Characters;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class MeltingCard : ShumitCard
{
    private const string DrawCountKey = "DrawCount";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(DrawCountKey, 2m),
    };

    public MeltingCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            Owner,
            prefs,
            (CardModel c) => !ReferenceEquals(c, this),
            this);
        CardModel? victim = picked.FirstOrDefault();
        if (victim != null)
        {
            await CardCmd.Exhaust(choiceContext, victim);
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars[DrawCountKey].IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DrawCountKey].UpgradeValueBy(1m);
    }
}
