using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;

namespace Frontier.Cards.Rare;

public abstract class SkeletonRareCard : ShumitCard
{
    private readonly string _title;
    private readonly string _desc;
    protected SkeletonRareCard(int cost, CardType type, string title, string desc) : base(cost, type, CardRarity.Rare, TargetType.None) { _title = title; _desc = desc; }
    public override List<(string, string)> Localization => new() { ("title", _title), ("description", _desc) };
}

[Pool(typeof(EventCardPool))] public sealed class RefiningCard : SkeletonRareCard { public RefiningCard() : base(2, CardType.Attack, "정련", "피해를 12 줍니다. 처치 시 손패 카드 1장을 영구 강화합니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class AnvilMemoryCard : SkeletonRareCard { public AnvilMemoryCard() : base(2, CardType.Attack, "모루의 기억", "보존. 피해를 3씩 15번 줍니다. 재련.") { } public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain }; }
[Pool(typeof(EventCardPool))] public sealed class BurningStrikeCard : SkeletonRareCard { public BurningStrikeCard() : base(1, CardType.Attack, "불태우는 일격", "피해를 8 주고 열기 5를 얻습니다. 재련.") { } }
[Pool(typeof(EventCardPool))] public sealed class FaultBreakCard : SkeletonRareCard { public FaultBreakCard() : base(3, CardType.Attack, "단층 파괴", "모든 적에게 피해 26. 방어도가 있으면 2배, 이후 방어도 파괴.") { } }
[Pool(typeof(EventCardPool))] public sealed class ImpurityCompressionCard : SkeletonRareCard { public ImpurityCompressionCard() : base(2, CardType.Attack, "불순물 압착", "모든 적에게 피해 후 덱의 화상 소멸, 소멸 수만큼 추가 피해.") { } }
[Pool(typeof(EventCardPool))] public sealed class UntilExhaustionCard : SkeletonRareCard { public UntilExhaustionCard() : base(0, CardType.Attack, "지쳐 쓰러질 때 까지", "X코스트. X회 공격, X장 강화, X회 열기 획득 후 턴 종료.") { } }

[Pool(typeof(EventCardPool))]
public sealed class ThermalEnergyConversionCard : ShumitCard
{
    public override List<(string, string)> Localization => new()
    {
        ("title", "열에너지 전환"),
        ("description", "X코스트. 열기 20당 에너지를 1 얻고, 열기를 전부 제거한 뒤 카드를 X장 뽑습니다. 소멸.")
    };
    public ThermalEnergyConversionCard() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None) { }
}

[Pool(typeof(EventCardPool))] public sealed class ForgeBlueprintCard : SkeletonRareCard { public ForgeBlueprintCard() : base(2, CardType.Power, "대장간의 도면", "강화 5회 시 대장간/용광로 중 1장을 선택해 손으로 가져옵니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class MasterySmeltingCard : SkeletonRareCard { public MasterySmeltingCard() : base(1, CardType.Power, "숙련 제련", "전투 종료 후 카드 1장에 숙련 인챈트를 부여합니다.") { } }
[Pool(typeof(EventCardPool))] public sealed class EchoSmeltingCard : SkeletonRareCard { public EchoSmeltingCard() : base(2, CardType.Power, "메아리 제련", "전투 종료 후 카드 1장에 재사용 인챈트를 부여합니다.") { } }
