using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Frontier.Characters;

namespace Frontier.Cards;

// 융해
[Pool(typeof(ShumitCardPool))]
public sealed class MeltingCard : ShumitCard
{
    protected override bool IsPlayable =>
        base.IsPlayable && (HasFurnaceOrForge() || HeatAtLeast(100));

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

        int draw = CurrentUpgradeLevel > 0 ? 3 : 2;
        await CardPileCmd.Draw(choiceContext, draw, Owner);
    }

    private bool HasFurnaceOrForge()
    {
        return PileType.Hand.GetPile(Owner).Cards.Any(static (CardModel c) => c is BlastFurnaceCard || c is ForgeCard);
    }

    private bool HeatAtLeast(int min)
    {
        return (Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0) >= min;
    }
}
