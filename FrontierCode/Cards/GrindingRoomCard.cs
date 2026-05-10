using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using Frontier.Characters;
using Frontier.Powers;

namespace Frontier.Cards;

// 연마실 (1코 토큰): 보존, 턴 시작 시 손에 있으면 드로우·힘·민첩·당일 에너지 획득 -1.
[Pool(typeof(ShumitCardPool))]
public sealed class GrindingRoomCard : TokenCardBase
{
    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Retain };

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new EnergyVar("GrindingEnergyPenalty", 1) };

    public GrindingRoomCard()
        : base(1, CardType.Skill, TargetType.None)
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

        await CardPileCmd.Draw(choiceContext, 1, Owner);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<ShumitTurnEnergyPenaltyPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
