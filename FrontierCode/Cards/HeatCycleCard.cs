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

// 열기 순환
[Pool(typeof(ShumitCardPool))]
public sealed class HeatCycleCard : ShumitCard
{
    private const string HeatMoveKey = "HeatMove";
    private const string DrawCountKey = "DrawCount";

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(10m, ValueProp.Move),
        new DynamicVar(DrawCountKey, 1m),
        new DynamicVar(HeatMoveKey, 10m),
    };

    public HeatCycleCard()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        int n = DynamicVars[DrawCountKey].IntValue;
        await CardPileCmd.Draw(choiceContext, n, Owner);
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        decimal delta = DynamicVars[HeatMoveKey].BaseValue;
        if (heat < 70)
        {
            await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, delta, this);
        }
        else
        {
            await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, delta, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DrawCountKey].UpgradeValueBy(1m);
    }
}
