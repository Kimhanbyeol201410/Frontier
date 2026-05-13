using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 광란의 열기 — 멈출 수 없는 열기 «걸작» 변환 카드.
//   - 모든 적에게 피해를 2회 명중 (활력 등은 단일 AttackCommand 안에서 두 히트에 모두 적용).
//   - 열기 20당 히트당 추가 데미지(HeatStep, 기본 4) — 멈출 수 없는 열기 스케일링을 변환 후에도 약하게 계승.
//   - 발동 후 열기 HeatGain(기본 40) 획득.
//   - 재련 5: 강화 시 카운터 감소.
[Pool(typeof(ShumitCardPool))]
public sealed class FrenziedHeatCard : ShumitCard
{
    private const int HitCount = 2;
    private const int HeatChunk = 20;
    private const string HeatStepKey = "HeatStep";
    private const string HeatGainKey = "Heat";
    private const string ReforgeLeftKey = "ReforgeLeft";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move),
        new DynamicVar(HeatStepKey, 4m),
        new DynamicVar(HeatGainKey, 40m),
        new DynamicVar(ReforgeLeftKey, 5m),
    };

    public FrenziedHeatCard()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    /// <summary>걸작 변환으로만 획득. 보상·상점·전투 무작위 생성 모두 제외.</summary>
    public override bool CanBeGeneratedInCombat => false;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CombatState combatState = FrontierCombatStateHelper.RequireFor(Owner);

        // 열기 비례 추가 데미지는 가열된 망치 패턴과 동일하게 (열기 / 20) × HeatStep.
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        int step = System.Math.Max(1, DynamicVars[HeatStepKey].IntValue);
        decimal bonus = (heat / HeatChunk) * step;
        decimal total = DynamicVars.Damage.BaseValue + bonus;

        // 단일 AttackCommand 로 멀티히트 처리. VigorPower 등이 모든 히트에 적용되게.
        await DamageCmd.Attack(total)
            .WithHitCount(HitCount)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .Execute(choiceContext);

        await PowerCmd.Apply<HeatPower>(Owner.Creature, DynamicVars[HeatGainKey].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars[ReforgeLeftKey].UpgradeValueBy(-1m);
    }
}
