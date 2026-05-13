using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 멈출 수 없는 열기: 전체 피해 + «열기 N마다 피해 X 증가». «걸작 10» — 강화 10에서 «광란의 열기»로 변환.
// 신의 눈 보유 시 걸작 기준 -5 (강화 5에서 변환). 카드 인스턴스의 MasterpieceLeft 도 신의 눈 획득 시 -5 동기화된다.
//   - HeatDivisor는 고정(20), DamagePer가 강화 시 1 증가(피해 증가량 자체가 강해짐).
[Pool(typeof(ShumitCardPool))]
public sealed class UnstoppableHeatCard : ShumitCard
{
    private const string HeatDivisorKey = "HeatDivisor";
    private const string DamagePerKey = "DamagePer";
    private const string MasterpieceLeftKey = "MasterpieceLeft";

    // «걸작»은 description 안의 [gold]걸작[/gold] {MasterpieceLeft}. 만으로 노출.
    // CanonicalKeywords 등록 시 카드 하단 키워드 라벨에도 [걸작] 이 같이 떠 두 번 표기되므로 제외.
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (IHoverTip tip in base.ExtraHoverTips)
            {
                yield return tip;
            }
            yield return HoverTipFactory.FromKeyword(FrontierKeywords.Masterpiece);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(11m, ValueProp.Move),
        new DynamicVar(HeatDivisorKey, 20m),
        new DynamicVar(DamagePerKey, 1m),
        new DynamicVar(MasterpieceLeftKey, 10m),
    };

    public UnstoppableHeatCard()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        int divisor = System.Math.Max(1, DynamicVars[HeatDivisorKey].IntValue);
        decimal bonus = (heat / divisor) * DynamicVars[DamagePerKey].BaseValue;
        decimal total = DynamicVars.Damage.BaseValue + bonus;
        await DamageCmd.Attack(total).FromCard(this).TargetingAllOpponents(FrontierCombatStateHelper.RequireFor(Owner)).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DamagePerKey].UpgradeValueBy(1m);
        DynamicVars[MasterpieceLeftKey].UpgradeValueBy(-1m);
    }
}
