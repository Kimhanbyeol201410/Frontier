using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 냉온화상: 사용 불가. 플레이어 턴 종료 시 손패에 있으면 피해 3.
[Pool(typeof(ShumitCardPool))]
public sealed class ColdBurnStatusCard : ShumitCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Unplayable };

    public ColdBurnStatusCard()
        : base(-2, CardType.Status, CardRarity.Event, TargetType.None)
    {
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || Pile?.Type != PileType.Hand)
        {
            return;
        }

        await CreatureCmd.Damage(choiceContext, Owner.Creature, 3m, ValueProp.Unpowered, null, this);
    }
}
