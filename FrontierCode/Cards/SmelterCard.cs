using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Frontier.Characters;

namespace Frontier.Cards;

// 제련소 (0코 토큰): 보존, 턴 시작 시 손에 있으면 열기 감소 후 손패 1장 강화.
[Pool(typeof(ShumitCardPool))]
public sealed class SmelterCard : TokenCardBase
{
    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Retain };

    public SmelterCard()
        : base(0, CardType.Skill, TargetType.None)
    {
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this))
        {
            return;
        }

        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, 15m, null);

        CardModel? selected = await CardSelectCmd.FromHandForUpgrade(choiceContext, Owner, this);
        if (selected != null && !ReferenceEquals(selected, this))
        {
            CardCmd.Upgrade(selected, CardPreviewStyle.HorizontalLayout);
        }
    }
}
