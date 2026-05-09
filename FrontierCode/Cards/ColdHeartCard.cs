using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Frontier.Characters;

namespace Frontier.Cards;

// 차가운 심장
[Pool(typeof(ShumitCardPool))]
public sealed class ColdHeartCard : ShumitCard
{
    private const string HeatLossKey = "HeatLoss";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(HeatLossKey, 20m) };

    public ColdHeartCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FrontierHeatUtil.ReduceHeat(Owner.Creature, DynamicVars[HeatLossKey].BaseValue, this);
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatLossKey].UpgradeValueBy(10m);
    }
}
