using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 가열된 망치
[Pool(typeof(ShumitCardPool))]
public sealed class HeatedHammerCard : ShumitCard
{
    private const string StepKey = "HeatStep";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar(StepKey, 1m),
    };

    public HeatedHammerCard()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target);
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        int step = System.Math.Max(1, DynamicVars[StepKey].IntValue);
        decimal bonus = (heat / 10) * step;
        decimal total = DynamicVars.Damage.BaseValue + bonus;
        await DamageCmd.Attack(total).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars[StepKey].UpgradeValueBy(1m);
    }
}
