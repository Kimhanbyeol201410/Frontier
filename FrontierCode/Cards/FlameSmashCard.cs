using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Cards;

// 화염 강타
[Pool(typeof(ShumitCardPool))]
public sealed class FlameSmashCard : ShumitCard
{
    private const string HeatKey = "Heat";
    private const string VulnerableKey = "Vulnerable";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(11m, ValueProp.Move),
        new DynamicVar(HeatKey, 15m),
        new DynamicVar(VulnerableKey, 1m),
    };

    public FlameSmashCard()
        : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars[VulnerableKey].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<HeatPower>(Owner.Creature, DynamicVars[HeatKey].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars[VulnerableKey].UpgradeValueBy(1m);
    }
}
