using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Frontier.Characters;

namespace Frontier.Cards;

// 녹이기
[Pool(typeof(ShumitCardPool))]
public sealed class MeltCard : ShumitCard
{
    protected override bool IsPlayable =>
        base.IsPlayable && PileType.Hand.GetPile(Owner).Cards.Any(static (CardModel c) => c is ForgeCard or BlastFurnaceCard);

    public MeltCard()
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

        await FrontierHeatUtil.ApplyHeat(Owner.Creature, 20m, this);
        await CardCmd.Exhaust(choiceContext, this);
    }
}
