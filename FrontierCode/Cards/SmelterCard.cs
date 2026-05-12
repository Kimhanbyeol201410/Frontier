using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class SmelterCard : ShumitCard
{
    private const string HeatPerTurnKey = "HeatPerTurn";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[]
    {
        FrontierKeywords.PreserveTrigger,
        CardKeyword.Retain,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(HeatPerTurnKey, 20m),
    };

    public SmelterCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyEffect(choiceContext);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this))
        {
            return;
        }

        await ApplyEffect(choiceContext);
    }

    private async Task ApplyEffect(PlayerChoiceContext choiceContext)
    {
        CardModel? selected = await CardSelectCmd.FromHandForUpgrade(choiceContext, Owner, this);
        if (selected != null && !ReferenceEquals(selected, this))
        {
            CardCmd.Upgrade(selected, CardPreviewStyle.HorizontalLayout);
        }

        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, DynamicVars[HeatPerTurnKey].BaseValue, this);
    }
}
