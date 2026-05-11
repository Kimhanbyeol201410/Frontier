using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;
using Frontier.Utilities;

namespace Frontier.Cards;

// 불꽃 튀기기 — 재련 10: 강화할수록 설명의 남은 재련 수가 10→0으로 감소.
[Pool(typeof(ShumitCardPool))]
public sealed class SparkBurstCard : ShumitCard
{
    private const string ReforgeLeftKey = "ReforgeLeft";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar("Heat", 10m),
        new DynamicVar(ReforgeLeftKey, 10m),
    };

    public SparkBurstCard()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(FrontierCombatStateHelper.RequireFor(Owner)).Execute(choiceContext);
        await PowerCmd.Apply<HeatPower>(Owner.Creature, DynamicVars["Heat"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars[ReforgeLeftKey].UpgradeValueBy(-1m);
    }
}
