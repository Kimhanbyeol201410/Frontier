using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 멈출 수 없는 열기: 전체 피해 + 열기 비례 추가 피해.
[Pool(typeof(ShumitCardPool))]
public sealed class UnstoppableHeatCard : ShumitCard
{
    private const string HeatDivisorKey = "HeatDivisor";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(11m, ValueProp.Move),
        new DynamicVar(HeatDivisorKey, 20m),
    };

    public UnstoppableHeatCard()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        int divisor = System.Math.Max(1, DynamicVars[HeatDivisorKey].IntValue);
        decimal bonus = heat / divisor;
        decimal total = DynamicVars.Damage.BaseValue + bonus;
        await DamageCmd.Attack(total).FromCard(this).TargetingAllOpponents(FrontierCombatStateHelper.RequireFor(Owner)).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatDivisorKey].UpgradeValueBy(-10m);
    }
}
