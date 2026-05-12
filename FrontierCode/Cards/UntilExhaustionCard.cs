using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 지쳐 쓰러질 때까지: X 피해/강화/열기 반복 후 턴 종료.
[Pool(typeof(ShumitCardPool))]
public sealed class UntilExhaustionCard : ShumitCard
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyXVar(),
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar("HeatPer", 10m),
    };

    public UntilExhaustionCard()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (FrontierCombatStateHelper.TryGetFor(Owner) is not CombatState combatState)
        {
            throw new System.InvalidOperationException("UntilExhaustionCard requires CombatState.");
        }

        int x = ResolveEnergyXValue();
        if (x <= 0)
        {
            return;
        }

        decimal dmg = DynamicVars.Damage.BaseValue;
        decimal heatEach = DynamicVars["HeatPer"].BaseValue;

        for (int i = 0; i < x; i++)
        {
            if (!combatState.HittableEnemies.Any())
            {
                break;
            }

            await DamageCmd.Attack(dmg)
                .FromCard(this)
                .TargetingAllOpponents(combatState)
                .Execute(choiceContext);

            List<CardModel> upgradable = PileType.Hand.GetPile(Owner).Cards
                .Where((CardModel c) => !ReferenceEquals(c, this) && c.IsUpgradable)
                .ToList();
            if (upgradable.Count > 0)
            {
                CardCmd.Upgrade(upgradable, CardPreviewStyle.HorizontalLayout);
            }

            await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, heatEach, this);
        }

        PlayerCmd.EndTurn(Owner, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
