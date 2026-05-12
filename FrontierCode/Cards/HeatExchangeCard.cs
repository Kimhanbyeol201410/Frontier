using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class HeatExchangeCard : ShumitCard
{
    private const string HeatLossKey = "HeatLoss";

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(10m, ValueProp.Move),
        new DynamicVar(HeatLossKey, 10m),
    };

    public HeatExchangeCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        IEnumerable<CardModel> pick = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            (CardModel c) => !ReferenceEquals(c, this),
            this);
        CardModel? moveCard = pick.FirstOrDefault();
        if (moveCard != null)
        {
            await CardPileCmd.Add(moveCard, PileType.Draw, CardPilePosition.Top, this);
            CardCmd.Upgrade(moveCard, CardPreviewStyle.HorizontalLayout);
        }

        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, DynamicVars[HeatLossKey].BaseValue, this);
    }
}
