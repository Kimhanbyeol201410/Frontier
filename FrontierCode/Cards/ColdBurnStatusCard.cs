using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 냉온화상: 사용 불가. 내 턴 종료 시 손패에 있으면 체력을 HpLoss 만큼 잃음 (슈미트.md).
[Pool(typeof(ShumitCardPool))]
public sealed class ColdBurnStatusCard : ShumitCard
{
    private const string HpLossKey = "HpLoss";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Unplayable };

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(HpLossKey, 3m) };

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

        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars[HpLossKey].BaseValue, ValueProp.Unpowered, null, this);
    }
}
