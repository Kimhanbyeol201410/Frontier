using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Frontier.Relics;

[Pool(typeof(EventRelicPool))]
public sealed class BrokenForgeRelic : CustomRelicModel
{
    private const string DrawPenaltyKey = "DrawPenalty";
    private const string ForgeCountKey = "ForgeCount";
    public override RelicRarity Rarity => RelicRarity.Starter;
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(DrawPenaltyKey, 1m), new DynamicVar(ForgeCountKey, 1m) };
    public override List<(string, string)> Localization => new() { ("title", "부서진 대장간"), ("description", "전투 시작 시 뽑을 카드 더미에 [gold]대장간[/gold]을 {ForgeCount:diff()}장 추가합니다.") };

    public override async Task BeforeCombatStart()
    {
        for (int i = 0; i < base.DynamicVars[ForgeCountKey].IntValue; i++)
        {
            CardModel forge = base.Owner.RunState.CreateCard<ForgeCard>(base.Owner);
            await CardPileCmd.Add(forge, PileType.Draw, CardPilePosition.Random);
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
            CardModel selected = await CardSelectCmd.FromHandForUpgrade(choiceContext, base.Owner, this);
            if (selected == null || selected is ForgeCard) break;
            CardCmd.Upgrade(selected, CardPreviewStyle.HorizontalLayout);
        }
        Flash();
    }

    private bool HasForgeInHand() => PileType.Hand.GetPile(base.Owner).Cards.Any((CardModel c) => c is ForgeCard);

    private int GetForgeUpgradeCount()
    {
        IReadOnlyList<CardModel> forgeCards = PileType.Hand.GetPile(base.Owner).Cards.Where((CardModel c) => c is ForgeCard).ToList();
        if (forgeCards.Count == 0) return 0;
        return forgeCards.Max((CardModel c) => c.DynamicVars["UpgradesPerTurn"].IntValue);
    }
}
