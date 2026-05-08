using System.Collections.Generic;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Frontier.Cards.Status;

[Pool(typeof(EventCardPool))]
public sealed class ColdBurnStatusCard : ShumitCard
{
    public override List<(string, string)> Localization => new()
    {
        ("title", "냉온화상"),
        ("description", "사용불가. 내 턴 종료 시 이 카드가 손에 있다면 체력을 3 잃습니다.")
    };

    public ColdBurnStatusCard() : base(-2, CardType.Status, CardRarity.Event, TargetType.None) { }
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 추후 구현
}
