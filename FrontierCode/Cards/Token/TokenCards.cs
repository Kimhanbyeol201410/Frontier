using System.Collections.Generic;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Frontier.Cards.Token;

public abstract class TokenCardBase : ShumitCard
{
    protected TokenCardBase(int cost, CardType type, TargetType target = TargetType.None)
        : base(cost, type, CardRarity.Event, target, showInCardLibrary: false)
    {
    }
}

[Pool(typeof(EventCardPool))]
public sealed class GreatForgeCard : TokenCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };
    public override List<(string, string)> Localization => new()
    {
        ("title", "위대한 대장간"),
        ("description", "보존. 매 턴마다 카드를 1장 적게 뽑고, 손패에 있는 카드 1장을 이번 전투 동안 2번 강화합니다.")
    };
    public GreatForgeCard() : base(0, CardType.Skill) { }
}

[Pool(typeof(EventCardPool))]
public sealed class BlastFurnaceCard : TokenCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };
    public override List<(string, string)> Localization => new()
    {
        ("title", "용광로"),
        ("description", "보존. 매 턴마다 열기를 15 얻습니다. 카드 1장을 소멸시킵니다.")
    };
    public BlastFurnaceCard() : base(2, CardType.Skill) { }
}

[Pool(typeof(EventCardPool))]
public sealed class SmelterCard : TokenCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };
    public override List<(string, string)> Localization => new()
    {
        ("title", "제련소"),
        ("description", "보존. 매 턴마다 열기를 15 잃고 카드 1장을 강화합니다.")
    };
    public SmelterCard() : base(1, CardType.Skill) { }
}

[Pool(typeof(EventCardPool))]
public sealed class GrindingRoomCard : TokenCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };
    public override List<(string, string)> Localization => new()
    {
        ("title", "연마실"),
        ("description", "보존. 매 턴마다 얻는 에너지가 1 감소하고 카드를 1장 뽑습니다. 힘 1, 민첩 1을 얻습니다.")
    };
    public GrindingRoomCard() : base(1, CardType.Skill) { }
}

[Pool(typeof(EventCardPool))]
public sealed class IronSwordCard : TokenCardBase
{
    public override List<(string, string)> Localization => new()
    {
        ("title", "철검"),
        ("description", "힘을 1 얻습니다.")
    };
    public IronSwordCard() : base(1, CardType.Skill) { }
}

[Pool(typeof(EventCardPool))]
public sealed class IronArmorCard : TokenCardBase
{
    public override List<(string, string)> Localization => new()
    {
        ("title", "철갑옷"),
        ("description", "방어도를 4 얻습니다.")
    };
    public IronArmorCard() : base(1, CardType.Skill) { }
}

[Pool(typeof(EventCardPool))]
public sealed class IronShieldCard : TokenCardBase
{
    public override List<(string, string)> Localization => new()
    {
        ("title", "철방패"),
        ("description", "민첩을 1 얻습니다.")
    };
    public IronShieldCard() : base(1, CardType.Skill) { }
}
