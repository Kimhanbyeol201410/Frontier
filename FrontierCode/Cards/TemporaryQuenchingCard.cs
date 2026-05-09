using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Frontier.Characters;

namespace Frontier.Cards;

// 임시 담금질: 열기 감소, 소멸.
[Pool(typeof(ShumitCardPool))]
public sealed class TemporaryQuenchingCard : ShumitCard
{
    private const string HeatReductionKey = "HeatReduction";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(HeatReductionKey, 10m) };

    public TemporaryQuenchingCard()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FrontierHeatUtil.ReduceHeat(Owner.Creature, DynamicVars[HeatReductionKey].BaseValue, this);
        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatReductionKey].UpgradeValueBy(5m);
    }
}
