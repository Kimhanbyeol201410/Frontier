using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Frontier.Characters;

namespace Frontier.Cards;

// 대장간 시설의 도면
[Pool(typeof(ShumitCardPool))]
public sealed class ForgeFacilityBlueprintCard : ShumitCard
{
    protected override bool IsPlayable => base.IsPlayable && FrontierSession.UpgradesThisCombat >= 5;

    public ForgeFacilityBlueprintCard()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CombatState cs = Owner.Creature.CombatState
            ?? throw new System.InvalidOperationException("ForgeFacilityBlueprintCard requires CombatState.");
        CardModel a = cs.CreateCard<GrindingRoomCard>(Owner);
        CardModel b = cs.CreateCard<SmelterCard>(Owner);
        CardModel? pick = await CardSelectCmd.FromChooseACardScreen(choiceContext, new List<CardModel> { a, b }, Owner);
        if (pick != null)
        {
            await CardPileCmd.Add(pick, PileType.Hand, CardPilePosition.Bottom, this);
        }
    }
}
