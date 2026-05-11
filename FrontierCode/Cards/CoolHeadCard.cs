using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Powers;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Frontier.Cards;

// 머리 식히기
[Pool(typeof(ShumitCardPool))]
public sealed class CoolHeadCard : ShumitCard
{
    private const string HeatLossKey = "HeatLoss";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(HeatLossKey, 10m),
    };

    public CoolHeadCard()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        decimal heat = DynamicVars[HeatLossKey].BaseValue;
        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, heat, this);
        await PowerCmd.Apply<ShumitCoolHeadNextTurnPower>(Owner.Creature, heat, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatLossKey].UpgradeValueBy(5m);
    }
}
