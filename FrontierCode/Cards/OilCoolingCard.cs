using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Cards;
using Frontier.Characters;

// PDF 원문(슈미트.pdf) 기준 카드 정보
// 유냉 (1코 / 스킬)
// - 방어도 7, 열기 -5
// 업그레이드: 방어도 9, 열기 -10
[Pool(typeof(ShumitCardPool))]
public sealed class OilCoolingCard : ShumitCard
{
    private const string HeatReductionKey = "HeatReduction";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(7m, ValueProp.Move),
        new DynamicVar(HeatReductionKey, 5m),
    };

    public OilCoolingCard()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        int currentHeat = base.Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        decimal reduceBy = System.Math.Min((decimal)currentHeat, base.DynamicVars[HeatReductionKey].BaseValue);
        if (reduceBy > 0m)
        {
            await PowerCmd.Apply<HeatPower>(base.Owner.Creature, -reduceBy, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(3m);
        base.DynamicVars[HeatReductionKey].UpgradeValueBy(5m);
    }
}



