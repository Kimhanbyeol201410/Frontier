using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Frontier.Cards;

// 제련 설계: N장 뽑고 뽑은 카드를 모두 1번 강화. 강화 시 드로우 +1.
[Pool(typeof(ShumitCardPool))]
public sealed class SmeltingDesignCard : ShumitCard
{
    private const string DrawCountKey = "Cards";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar(DrawCountKey, 1m),
    };

    public SmeltingDesignCard()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int drawCount = DynamicVars[DrawCountKey].IntValue;
        if (drawCount <= 0)
        {
            return;
        }

        IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, drawCount, Owner);
        foreach (CardModel c in drawn.ToList())
        {
            if (c.IsUpgradable)
            {
                CardCmd.Upgrade(c, CardPreviewStyle.HorizontalLayout);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DrawCountKey].UpgradeValueBy(1m);
    }
}
