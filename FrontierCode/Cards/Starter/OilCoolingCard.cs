using System.Collections.Generic;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Frontier.Cards.Starter;

[Pool(typeof(EventCardPool))]
public sealed class OilCoolingCard : ShumitCard
{
    public override List<(string, string)> Localization => new()
    {
        ("title", "유냉"),
        ("description", "방어도를 7 얻습니다. 열기를 5 잃습니다.")
    };

    public OilCoolingCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.None) { }
}
