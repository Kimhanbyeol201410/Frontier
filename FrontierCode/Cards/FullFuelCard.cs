using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Powers;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Cards;

// 연료 최대로
[Pool(typeof(ShumitCardPool))]
public sealed class FullFuelCard : ShumitCard
{
    private const string NextTurnEnergyKey = "NextTurnEnergy";

    public override bool GainsBlock => true;

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(6m, ValueProp.Move),
        new EnergyVar(NextTurnEnergyKey, 1),
    };

    public FullFuelCard()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        if (heat > 0)
        {
            await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, heat, this);
        }

        decimal nextEnergy = DynamicVars[NextTurnEnergyKey].BaseValue;
        await PowerCmd.Apply<ShumitFuelMaxNextTurnEnergyPower>(Owner.Creature, nextEnergy, Owner.Creature, this);
        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[NextTurnEnergyKey].UpgradeValueBy(1m);
    }
}
