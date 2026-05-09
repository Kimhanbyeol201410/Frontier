using System.Collections.Generic;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Frontier.Cards;

public abstract class SkeletonRareCard : ShumitCard
{
    protected SkeletonRareCard(int cost, CardType type, string title, string desc)
        : base(cost, type, CardRarity.Rare, TargetType.None)
    {
    }

    public override List<(string, string)> Localization => new() { ("title", GetType().Name), ("description", GetType().Name + " card.") };
}


