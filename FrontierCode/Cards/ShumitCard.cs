using BaseLib.Abstracts;
using Frontier.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Frontier.Cards;

public abstract class ShumitCard : CustomCardModel
{
    protected ShumitCard(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true)
        : base(cost, type, rarity, target, showInCardLibrary)
    {
    }

    public override string PortraitPath => $"{Id.Entry.RemoveFrontierPrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string CustomPortraitPath => $"{Id.Entry.RemoveFrontierPrefix().ToLowerInvariant()}.png".BigCardImagePath();
}
