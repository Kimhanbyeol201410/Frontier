using System.Collections.Generic;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Frontier.Cards.Common;

public abstract class SkeletonCard : ShumitCard
{
    private readonly string _title;
    private readonly string _description;
    protected SkeletonCard(int cost, CardType type, string title, string description)
        : base(cost, type, CardRarity.Common, TargetType.None)
    {
        _title = title;
        _description = description;
    }
    public override List<(string, string)> Localization => new() { ("title", _title), ("description", _description) };
}

[Pool(typeof(EventCardPool))] public sealed class HammerDownCard : SkeletonCard { public HammerDownCard() : base(1, CardType.Attack, "내려찍기", "피해를 7 줍니다. 취약을 1 부여합니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class TapCard : SkeletonCard { public TapCard() : base(0, CardType.Attack, "두드리기", "피해를 3 줍니다. 열기를 5 얻습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class DoubleTapCard : SkeletonCard { public DoubleTapCard() : base(1, CardType.Attack, "두 번 두드리기", "피해를 5만큼 2번 줍니다. 열기를 10 얻습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class ColdGestureCard : SkeletonCard { public ColdGestureCard() : base(0, CardType.Attack, "차가운 손짓", "피해를 3 줍니다. 열기를 5 잃습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class CoolingStrikeCard : SkeletonCard { public CoolingStrikeCard() : base(1, CardType.Attack, "냉각 타격", "피해를 6 줍니다. 열기를 10 잃습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class HeatedHammerCard : SkeletonCard { public HeatedHammerCard() : base(1, CardType.Attack, "가열된 망치", "피해를 5 줍니다. 열기 10마다 추가 피해를 줍니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class FlameStrikeCard : SkeletonCard { public FlameStrikeCard() : base(1, CardType.Attack, "화염 타격", "피해를 6 줍니다. 열기를 10 얻습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class FlamePulseCard : SkeletonCard { public FlamePulseCard() : base(1, CardType.Attack, "화염 파동", "모든 적에게 피해를 6 줍니다. 열기를 10 얻습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class FlameSmashCard : SkeletonCard { public FlameSmashCard() : base(2, CardType.Attack, "화염 강타", "피해를 10 줍니다. 열기를 15 얻습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class SmeltingStrikeCard : SkeletonCard { public SmeltingStrikeCard() : base(1, CardType.Attack, "제련 타격", "피해를 4 줍니다. 강화되지 않은 카드 1장을 강화하고 열기를 10 얻습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class MaterialGatherCard : SkeletonCard { public MaterialGatherCard() : base(1, CardType.Attack, "재료 수급", "피해를 9 줍니다. 카드를 1장 뽑습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class SturdyArmorCard : SkeletonCard { public SturdyArmorCard() : base(1, CardType.Skill, "단단한 갑옷", "방어도 8을 얻습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class HeatedShieldCard : SkeletonCard { public HeatedShieldCard() : base(1, CardType.Skill, "달궈진 방패", "방어도 6을 얻고 열기를 10 얻습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class UnburningBodyCard : SkeletonCard { public UnburningBodyCard() : base(2, CardType.Skill, "불사르지 않는 몸", "방어도 11을 얻고 화상 1장을 버린 더미에 추가합니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class BellowsCard : SkeletonCard { public BellowsCard() : base(1, CardType.Skill, "풀무질", "열기를 15 얻고 카드를 1장 뽑습니다. 재련.") { } }
[Pool(typeof(EventCardPool))] public sealed class ImpurityRemovalCard : SkeletonCard { public ImpurityRemovalCard() : base(1, CardType.Skill, "불순물 제거", "방어도 5를 얻고 화상 카드 1장을 소멸시킵니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class TemporaryQuenchingCard : SkeletonCard { public TemporaryQuenchingCard() : base(0, CardType.Skill, "임시 담금질", "열기를 10 감소시킵니다. 소멸.") { } }
[Pool(typeof(EventCardPool))] public sealed class TongsCard : SkeletonCard { public TongsCard() : base(0, CardType.Skill, "집게질", "손패 카드 1장에 열기 5를 부여합니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class HeatCycleCard : SkeletonCard { public HeatCycleCard() : base(1, CardType.Skill, "열기 순환", "카드를 1장 뽑고 열기 상태에 따라 10을 얻거나 잃습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class FurnaceMaintenanceCard : SkeletonCard { public FurnaceMaintenanceCard() : base(2, CardType.Skill, "가열로 정비", "방어도 8을 얻고 열기 10을 얻습니다. 소멸.") { } }
[Pool(typeof(EventCardPool))] public sealed class WaterCoolingCard : SkeletonCard { public WaterCoolingCard() : base(1, CardType.Skill, "수냉", "방어도 7을 얻고 열기 15를 감소시킵니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class SmeltingDesignCard : SkeletonCard { public SmeltingDesignCard() : base(1, CardType.Skill, "제련 설계", "카드를 1장 뽑고 다음 공격 카드에 열기 5를 부여합니다.") { } }
