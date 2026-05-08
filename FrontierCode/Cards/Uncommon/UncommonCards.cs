using System.Collections.Generic;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Frontier.Cards.Uncommon;

public abstract class SkeletonUncommonCard : ShumitCard
{
    private readonly string _title;
    private readonly string _desc;
    protected SkeletonUncommonCard(int cost, CardType type, string title, string desc) : base(cost, type, CardRarity.Uncommon, TargetType.None) { _title = title; _desc = desc; }
    public override List<(string, string)> Localization => new() { ("title", _title), ("description", _desc) };
}

[Pool(typeof(EventCardPool))] public sealed class SparkBurstCard : SkeletonUncommonCard { public SparkBurstCard() : base(2, CardType.Attack, "불꽃 튀기기", "모든 적에게 피해를 7 줍니다. 열기 10 획득. 재련 5.") { } }
[Pool(typeof(EventCardPool))] public sealed class HeatingCard : SkeletonUncommonCard { public HeatingCard() : base(1, CardType.Attack, "가열", "보존. 피해 10. 열기 0이면 비용 0 및 드로우. 열기 15 획득.") { } public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain }; }
[Pool(typeof(EventCardPool))] public sealed class AnvilEchoCard : SkeletonUncommonCard { public AnvilEchoCard() : base(2, CardType.Attack, "모루의 잔향", "피해 2를 5번. 걸작 10.") { } }
[Pool(typeof(EventCardPool))] public sealed class UnstoppableHeatCard : SkeletonUncommonCard { public UnstoppableHeatCard() : base(2, CardType.Attack, "멈출 수 없는 열기", "모든 적에게 피해 11. 열기 기반 추가 피해.") { } }
[Pool(typeof(EventCardPool))] public sealed class HeatExchangeCard : SkeletonUncommonCard { public HeatExchangeCard() : base(1, CardType.Attack, "열 교환", "피해 8. 카드 1장을 덱 위로 보내고 강화. 열기 10 감소.") { } }
[Pool(typeof(EventCardPool))] public sealed class CrushingHammerCard : SkeletonUncommonCard { public CrushingHammerCard() : base(1, CardType.Attack, "파쇄의 망치질", "피해 10. 방어도 제거. 열기 5 획득.") { } }
[Pool(typeof(EventCardPool))] public sealed class MasterpieceHammerCard : SkeletonUncommonCard { public MasterpieceHammerCard() : base(2, CardType.Attack, "걸작을 위한 망치질", "피해 24. 열기 30 획득.") { } }
[Pool(typeof(EventCardPool))] public sealed class ApproachingDreadCard : SkeletonUncommonCard { public ApproachingDreadCard() : base(1, CardType.Attack, "다가가는 공포", "피해 6. 이번 턴 열기 20마다 힘 증가.") { } }
[Pool(typeof(EventCardPool))] public sealed class MasterPrideCard : SkeletonUncommonCard { public MasterPrideCard() : base(3, CardType.Skill, "명인의 긍지", "손패/드로우 더미 전체 강화. 열기 50 미만이면 50으로 변경. 소멸.") { } }
[Pool(typeof(EventCardPool))] public sealed class ColdHeartCard : SkeletonUncommonCard { public ColdHeartCard() : base(1, CardType.Skill, "차가운 심장", "열기 20 감소, 카드 1장 드로우.") { } }
[Pool(typeof(EventCardPool))] public sealed class FoldedSteelCard : SkeletonUncommonCard { public FoldedSteelCard() : base(1, CardType.Skill, "접쇠", "열기 50 이상 사용 가능. 강화 카드 재사용.") { } }
[Pool(typeof(EventCardPool))] public sealed class FirePowerPlantCard : SkeletonUncommonCard { public FirePowerPlantCard() : base(1, CardType.Skill, "화력발전기", "카드 1장 소멸. 화상이면 에너지 획득.") { } }
[Pool(typeof(EventCardPool))] public sealed class MeltCard : SkeletonUncommonCard { public MeltCard() : base(1, CardType.Skill, "융해", "용광로 보유 또는 열기 100 이상일 때 사용 가능.") { } }
[Pool(typeof(EventCardPool))] public sealed class MeltingCard : SkeletonUncommonCard { public MeltingCard() : base(1, CardType.Skill, "녹이기", "대장간/용광로 필요. 카드 소멸, 열기 20 획득. 소멸.") { } }
[Pool(typeof(EventCardPool))] public sealed class ManufactureCard : SkeletonUncommonCard { public ManufactureCard() : base(1, CardType.Skill, "제조", "대장간 필요. 철검/철갑옷/철방패 생성.") { } }
[Pool(typeof(EventCardPool))] public sealed class AbsoluteZeroCard : SkeletonUncommonCard { public AbsoluteZeroCard() : base(2, CardType.Skill, "절대영도", "열기/신체화상 제거, 방어도 획득, 냉온화상 추가, 소멸.") { } }
[Pool(typeof(EventCardPool))] public sealed class MetalLiquefactionCard : SkeletonUncommonCard { public MetalLiquefactionCard() : base(2, CardType.Skill, "금속 액화", "강화 공격 카드 비용 0, 열기 10 획득, 화상 2장 추가.") { } }
[Pool(typeof(EventCardPool))] public sealed class DesignCompletionCard : SkeletonUncommonCard { public DesignCompletionCard() : base(1, CardType.Skill, "설계의 완성", "드로우 더미에서 카드 선택해 손패로 가져오고 강화. 소멸.") { } }
[Pool(typeof(EventCardPool))] public sealed class ReuseAestheticsCard : SkeletonUncommonCard { public ReuseAestheticsCard() : base(1, CardType.Skill, "재사용의 미학", "이번 턴 소멸한 화상 수만큼 드로우.") { } }
[Pool(typeof(EventCardPool))] public sealed class ForgeFacilityBlueprintCard : SkeletonUncommonCard { public ForgeFacilityBlueprintCard() : base(2, CardType.Power, "대장간 시설의 도면", "강화 5회 시 연마실/제련소 선택 생성.") { } }
[Pool(typeof(EventCardPool))] public sealed class HeartOfFlameCard : SkeletonUncommonCard { public HeartOfFlameCard() : base(3, CardType.Power, "화염의 심장", "전투 동안 화상/신체화상 피해를 받지 않습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class FlameArmorCard : SkeletonUncommonCard { public FlameArmorCard() : base(2, CardType.Power, "화염의 갑옷", "화상이 덱에 추가될 때마다 방어도 획득.") { } }
[Pool(typeof(EventCardPool))] public sealed class SharpSmeltingCard : SkeletonUncommonCard { public SharpSmeltingCard() : base(1, CardType.Power, "예리한 제련", "전투 종료 후 카드 1장에 예리 인챈트.") { } }
[Pool(typeof(EventCardPool))] public sealed class AgileSmeltingCard : SkeletonUncommonCard { public AgileSmeltingCard() : base(1, CardType.Power, "기민함 제련", "전투 종료 후 카드 1장에 기민함 인챈트.") { } }
[Pool(typeof(EventCardPool))] public sealed class CoolingSystemCard : SkeletonUncommonCard { public CoolingSystemCard() : base(1, CardType.Power, "냉각 시스템", "턴 종료 시 열기 감소.") { } }
[Pool(typeof(EventCardPool))] public sealed class HeatedForgeCard : SkeletonUncommonCard { public HeatedForgeCard() : base(1, CardType.Power, "뜨거워진 대장간", "턴 종료 시 열기 증가.") { } }
[Pool(typeof(EventCardPool))] public sealed class ExhaustSystemCard : SkeletonUncommonCard { public ExhaustSystemCard() : base(3, CardType.Power, "배기 시스템", "턴 시작 시 열기 조건 만족하면 열기 감소 + 드로우.") { } }
[Pool(typeof(EventCardPool))] public sealed class FearlessOfFlameCard : SkeletonUncommonCard { public FearlessOfFlameCard() : base(2, CardType.Power, "화염을 무서워하지 않는 자", "매 턴 화상 1장을 얻습니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class SteamReleaseCard : SkeletonUncommonCard { public SteamReleaseCard() : base(2, CardType.Power, "증기 배출", "열기 감소 시 적 전체 피해. 재련 10.") { } }
