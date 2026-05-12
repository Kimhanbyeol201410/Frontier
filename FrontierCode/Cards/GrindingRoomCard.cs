using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class GrindingRoomCard : ShumitCard
{
    private const string VigorAmountKey = "VigorAmount";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[]
    {
        FrontierKeywords.PreserveTrigger,
        CardKeyword.Retain,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(VigorAmountKey, 6m),
    };

    public GrindingRoomCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyEffect(choiceContext);
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

        await ApplyEffect(choiceContext);
    }

    private async Task ApplyEffect(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<VigorPower>(Owner.Creature, DynamicVars[VigorAmountKey].BaseValue, Owner.Creature, this);
    }
}
