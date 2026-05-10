using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using Frontier.Utilities;

namespace Frontier.Relics;

[Pool(typeof(EventRelicPool))]
public sealed class BrokenForgeRelic : CustomRelicModel
{
    private const string DrawPenaltyKey = "DrawPenalty";
    private const string ForgeCountKey = "ForgeCount";
    public override RelicRarity Rarity => RelicRarity.Starter;
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(DrawPenaltyKey, 1m), new DynamicVar(ForgeCountKey, 1m) };

    public override async Task BeforeCombatStart()
    {
        List<CardModel> generated = new();
        if (FrontierCombatStateHelper.TryGetFor(base.Owner) is not CombatState combatState)
        {
            throw new InvalidOperationException("BrokenForgeRelic.BeforeCombatStart requires an active CombatState.");
        }
        for (int i = 0; i < base.DynamicVars[ForgeCountKey].IntValue; i++)
        {
            generated.Add(combatState.CreateCard<ForgeCard>(base.Owner));
        }

        if (generated.Count > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(generated, PileType.Draw, addedByPlayer: true, CardPilePosition.Random);
        }
        Flash();
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != base.Owner || !HasForgeInHand()) return count;
        return Math.Max(0m, count - base.DynamicVars[DrawPenaltyKey].BaseValue);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner) return;
        int upgradeCount = GetForgeUpgradeCount();
        if (upgradeCount <= 0) return;

        for (int i = 0; i < upgradeCount; i++)
        {
            if (!FrontierHandForgeUpgrade.TryUpgradeOneRandomFromHand(base.Owner))
            {
                break;
            }
        }
        Flash();
    }

    private bool HasForgeInHand() => PileType.Hand.GetPile(base.Owner).Cards.Any((CardModel c) => c is ForgeCard or GreatForgeCard);

    private int GetForgeUpgradeCount()
    {
        IReadOnlyList<CardModel> forgeCards = PileType.Hand.GetPile(base.Owner).Cards.Where((CardModel c) => c is ForgeCard or GreatForgeCard).ToList();
        if (forgeCards.Count == 0) return 0;
        return forgeCards.Max((CardModel c) => c.DynamicVars["UpgradesPerTurn"].IntValue);
    }
}

