using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;
using Frontier.Utilities;

namespace Frontier.Cards;

// 가열
[Pool(typeof(ShumitCardPool))]
public sealed class HeatingCard : ShumitCard
{
    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Retain };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
        new DynamicVar("DrawWhenHeatZero", 1m),
        new DynamicVar("HeatGain", 15m),
    };

    public HeatingCard()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (!ReferenceEquals(card, this))
        {
            modifiedCost = originalCost;
            return false;
        }

        Player? owner = Owner;
        if (owner?.Creature is not { } creature)
        {
            modifiedCost = originalCost;
            return false;
        }

        int h = creature.GetPower<HeatPower>()?.Amount ?? 0;
        if (h == 0)
        {
            modifiedCost = 0m;
            return true;
        }

        modifiedCost = originalCost;
        return false;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target);
        int heatBefore = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        if (heatBefore == 0)
        {
            int draws = DynamicVars["DrawWhenHeatZero"].IntValue;
            await CardPileCmd.Draw(choiceContext, draws, Owner);
        }

        await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, DynamicVars["HeatGain"].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["DrawWhenHeatZero"].UpgradeValueBy(1m);
    }
}
