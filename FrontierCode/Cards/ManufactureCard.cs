using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Frontier.Characters;

namespace Frontier.Cards;

// 제조
[Pool(typeof(ShumitCardPool))]
public sealed class ManufactureCard : ShumitCard
{
    protected override bool IsPlayable =>
        base.IsPlayable && PileType.Hand.GetPile(Owner).Cards.Any(static (CardModel c) => c is ForgeCard);

    public ManufactureCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CombatState combatState = Owner.Creature.CombatState
            ?? throw new System.InvalidOperationException("ManufactureCard requires CombatState.");
        int count = CurrentUpgradeLevel > 0 ? 2 : 1;
        for (int i = 0; i < count; i++)
        {
            int r = combatState.RunState.Rng.Shuffle.NextInt(3);
            CardModel token = r switch
            {
                0 => combatState.CreateCard<IronSwordCard>(Owner),
                1 => combatState.CreateCard<IronArmorCard>(Owner),
                _ => combatState.CreateCard<IronShieldCard>(Owner),
            };
            await CardPileCmd.Add(token, PileType.Hand, CardPilePosition.Bottom, this);
        }
    }
}
