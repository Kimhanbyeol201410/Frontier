using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;
using Frontier.Utilities;

namespace Frontier.Cards;

// 모루의 기억
//   - OnPlay: 모든 적에게 약화·취약 부여 후, 피해 3을 10회 명중.
//   - 디버프 부여는 공격 이전 단계에서 한 번에 처리.
[Pool(typeof(ShumitCardPool))]
public sealed class AnvilMemoryCard : ShumitCard
{
    private const string HitsKey = "Hits";
    private const string WeakKey = "Weak";
    private const string VulnKey = "Vulnerable";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Retain };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        new DynamicVar(HitsKey, 10m),
        new DynamicVar(WeakKey, 3m),
        new DynamicVar(VulnKey, 3m),
    };

    public AnvilMemoryCard()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    /// <summary>보상·상점은 <see cref="ShumitCardPool"/> 필터로 제외. 여기서는 전투 중 무작위 생성(포션·일부 효과)만 막는다. 걸작 변환은 <c>CreateCard</c>로 정상 생성.</summary>
    public override bool CanBeGeneratedInCombat => false;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CombatState combatState = FrontierCombatStateHelper.RequireFor(Owner);

        decimal weak = DynamicVars[WeakKey].BaseValue;
        decimal vuln = DynamicVars[VulnKey].BaseValue;
        foreach (Creature enemy in combatState.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<WeakPower>(enemy, weak, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(enemy, vuln, Owner.Creature, this);
        }

        int hits = DynamicVars[HitsKey].IntValue;
        for (int i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(combatState).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
