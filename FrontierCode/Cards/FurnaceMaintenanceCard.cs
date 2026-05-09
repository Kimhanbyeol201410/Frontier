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

// 가열로 정비
[Pool(typeof(ShumitCardPool))]
public sealed class FurnaceMaintenanceCard : ShumitCard
{
    private const string HeatGainKey = "HeatGain";

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(8m, ValueProp.Move),
        new DynamicVar(HeatGainKey, 10m),
    };

    public FurnaceMaintenanceCard()
        : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await FrontierHeatUtil.ApplyHeat(Owner.Creature, DynamicVars[HeatGainKey].BaseValue, this);
        if (FrontierSession.BurnCardExhaustedThisPlayerTurn)
        {
            await PlayerCmd.GainEnergy(2, Owner);
        }

        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
