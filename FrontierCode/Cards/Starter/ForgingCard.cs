using System.Collections.Generic;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Frontier.Cards.Starter;

[Pool(typeof(EventCardPool))]
public sealed class ForgingCard : ShumitCard
{
    public override List<(string, string)> Localization => new()
    {
        ("title", "단조"),
        ("description", "피해를 8 줍니다. 손패의 무작위 카드 1장을 이번 턴 동안 강화합니다. 열기를 10 얻습니다.")
    };

    public ForgingCard() : base(2, CardType.Attack, CardRarity.Common, TargetType.None) { }
}
