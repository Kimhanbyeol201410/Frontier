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

// 풀무질
[Pool(typeof(ShumitCardPool))]
public sealed class BellowsCard : ShumitCard
{
    private const string HeatKey = "HeatGain";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(HeatKey, 15m) };

    public BellowsCard()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FrontierHeatUtil.ApplyHeat(Owner.Creature, DynamicVars[HeatKey].BaseValue, this);
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatKey].UpgradeValueBy(5m);
    }
}
