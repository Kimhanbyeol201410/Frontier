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

// 광란의 열기 — 멈출 수 없는 열기 «걸작 5» 변환 카드.
//   - 모든 적에게 피해를 2회 명중.
//   - 매 명중 시 «열기 HeatDivisor마다 DamagePer» 만큼 추가 피해.
//   - 강화: DamagePer +1 (기본 6 → 7 → 8…). base damage·HeatDivisor는 고정.
[Pool(typeof(ShumitCardPool))]
public sealed class FrenziedHeatCard : ShumitCard
{
    private const string HeatDivisorKey = "HeatDivisor";
    private const string DamagePerKey = "DamagePer";
    private const int HitCount = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(22m, ValueProp.Move),
        new DynamicVar(HeatDivisorKey, 5m),
        new DynamicVar(DamagePerKey, 6m),
    };

    public FrenziedHeatCard()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    /// <summary>걸작 변환으로만 획득. 보상·상점·전투 무작위 생성 모두 제외.</summary>
    public override bool CanBeGeneratedInCombat => false;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        int divisor = System.Math.Max(1, DynamicVars[HeatDivisorKey].IntValue);
        decimal bonus = (heat / divisor) * DynamicVars[DamagePerKey].BaseValue;
        decimal total = DynamicVars.Damage.BaseValue + bonus;
        CombatState combatState = FrontierCombatStateHelper.RequireFor(Owner);
        for (int i = 0; i < HitCount; i++)
        {
            await DamageCmd.Attack(total).FromCard(this).TargetingAllOpponents(combatState).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DamagePerKey].UpgradeValueBy(1m);
    }
}
