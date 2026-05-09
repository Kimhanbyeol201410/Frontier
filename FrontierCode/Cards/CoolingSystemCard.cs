using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Frontier.Characters;
using Frontier.Powers;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class CoolingSystemCard : ShumitCard
{
    private const string HeatLossKey = "HeatLoss";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(HeatLossKey, 5m) };

    public CoolingSystemCard()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ShumitCoolingSystemPower>(
            Owner.Creature,
            DynamicVars[HeatLossKey].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatLossKey].UpgradeValueBy(5m);
    }
}
