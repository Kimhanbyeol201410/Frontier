using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Frontier.Cards;

namespace Frontier.Utilities;

/// <summary>대장간 계열: 손패에서 강화 대상을 고르지 않고 무작위로 1장 강화.</summary>
internal static class FrontierHandForgeUpgrade
{
	internal static bool TryUpgradeOneRandomFromHand(Player player, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
	{
		List<CardModel> candidates = PileType.Hand.GetPile(player).Cards
			.Where(static c => c is not ForgeCard && c.IsUpgradable)
			.ToList();
		if (candidates.Count == 0)
		{
			return false;
		}

		CardModel pick = player.RunState.Rng.CombatCardGeneration.NextItem(candidates)!;
		CardCmd.Upgrade(pick, style);
		return true;
	}
}
