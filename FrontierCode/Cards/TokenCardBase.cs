using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Frontier.Cards;

public abstract class TokenCardBase : ShumitCard
{
    protected TokenCardBase(int cost, CardType type, TargetType target = TargetType.None)
        : base(cost, type, CardRarity.Event, target, showInCardLibrary: false)
    {
    }
}


