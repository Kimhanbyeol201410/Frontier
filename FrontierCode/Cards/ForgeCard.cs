using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class ForgeCard : ShumitCard
{
    private const string UpgradesPerTurnKey = "UpgradesPerTurn";
    private const string HeatPerTurnKey = "HeatPerTurn";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[]
    {
        FrontierKeywords.PreserveTrigger,
        CardKeyword.Retain,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(UpgradesPerTurnKey, 2m),
        new DynamicVar(HeatPerTurnKey, 10m),
    };

    public ForgeCard()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
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
        int times = DynamicVars[UpgradesPerTurnKey].IntValue;
        for (int i = 0; i < times; i++)
        {
            if (!FrontierHandForgeUpgrade.TryUpgradeOneRandomFromHand(Owner))
            {
                break;
            }
        }

        await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, DynamicVars[HeatPerTurnKey].BaseValue, this);
    }
}
