using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;
using Frontier.Utilities;

namespace Frontier.Cards;

// PDF 원문(슈미트.pdf) 기준 카드 정보
// 두 번 두드리기 (1코 / 공격)
// - 피해 5를 2회, 열기 +10
// 업그레이드: 피해 7을 2회
[Pool(typeof(ShumitCardPool))]
public sealed class DoubleTapCard : ShumitCard
{
    private const string HitsKey = "Hits";

    private const string HeatKey = "HeatGain";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar(HitsKey, 2m),
        new DynamicVar(HeatKey, 10m),
    };

    public DoubleTapCard()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        // 단일 AttackCommand 로 멀티히트 처리. VigorPower 등이 모든 히트에 적용되게.
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(base.DynamicVars[HitsKey].IntValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await FrontierHeatUtil.ApplyHeat(choiceContext, base.Owner.Creature, base.DynamicVars[HeatKey].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
    }
}



