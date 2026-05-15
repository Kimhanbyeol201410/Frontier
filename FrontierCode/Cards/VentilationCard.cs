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

// 환기
[Pool(typeof(ShumitCardPool))]
public sealed class VentilationCard : ShumitCard
{
    private const string HeatPerCardKey = "HeatPerCard";
    private const string BlockPerCardKey = "BlockPerCard";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(HeatPerCardKey, 5m),
        new DynamicVar(BlockPerCardKey, 3m),
    };

    public VentilationCard()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        decimal heatPer = DynamicVars[HeatPerCardKey].BaseValue;
        ShumitVentilationThisTurnPower? power = await PowerCmd.Apply<ShumitVentilationThisTurnPower>(Owner.Creature, heatPer, Owner.Creature, this);
        if (power != null)
        {
            power.BlockPerCard = DynamicVars[BlockPerCardKey].IntValue;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatPerCardKey].UpgradeValueBy(5m);
    }
}
