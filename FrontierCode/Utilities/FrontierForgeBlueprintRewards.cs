using System.Collections.Generic;
using System.Threading.Tasks;
using Frontier.Cards;
using Frontier.Characters;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Utilities;

/// <summary>
/// 대장간의 도면 / 대장간 시설의 도면: 강화 5회 도달 시 토큰 선택 보상.
/// </summary>
internal static class FrontierForgeBlueprintRewards
{
	internal static void OfferAfterThresholdIfQueuedDeferred(Player owner)
	{
		if (owner == null || CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}

		if (owner.Character?.Id?.Entry != ShumitCharacter.CharacterId)
		{
			return;
		}

		if (FrontierSession.GetUpgradesThisCombat(owner) < 5)
		{
			return;
		}

		Callable.From(() => _ = OfferAfterThresholdIfQueuedAsync(owner)).CallDeferred();
	}

	private static async Task OfferAfterThresholdIfQueuedAsync(Player owner)
	{
		if (owner == null || CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}

		if (FrontierSession.GetUpgradesThisCombat(owner) < 5)
		{
			return;
		}

		PlayerChoiceContext ctx = new BlockingPlayerChoiceContext();

		if (FrontierSession.TryConsumeQueuedForgeMainBlueprint(owner))
		{
			await OfferMainPairAsync(owner, ctx);
		}

		if (FrontierSession.TryConsumeQueuedForgeFacilityBlueprint(owner))
		{
			await OfferFacilityPairAsync(owner, ctx);
		}
	}

	internal static async Task OfferMainPairAsync(Player owner, PlayerChoiceContext choiceContext)
	{
		if (FrontierCombatStateHelper.TryGetFor(owner) is not CombatState cs)
		{
			throw new System.InvalidOperationException("Forge blueprint reward requires CombatState.");
		}

		CardModel a = cs.CreateCard<ForgeCard>(owner);
		CardModel b = cs.CreateCard<BlastFurnaceCard>(owner);
		CardModel? pick = await CardSelectCmd.FromChooseACardScreen(choiceContext, new List<CardModel> { a, b }, owner);
		if (pick != null)
		{
			await CardPileCmd.Add(pick, PileType.Hand, CardPilePosition.Bottom, null);
		}
	}

	internal static async Task OfferFacilityPairAsync(Player owner, PlayerChoiceContext choiceContext)
	{
		if (FrontierCombatStateHelper.TryGetFor(owner) is not CombatState cs)
		{
			throw new System.InvalidOperationException("Forge facility blueprint reward requires CombatState.");
		}

		CardModel a = cs.CreateCard<GrindingRoomCard>(owner);
		CardModel b = cs.CreateCard<SmelterCard>(owner);
		CardModel? pick = await CardSelectCmd.FromChooseACardScreen(choiceContext, new List<CardModel> { a, b }, owner);
		if (pick != null)
		{
			await CardPileCmd.Add(pick, PileType.Hand, CardPilePosition.Bottom, null);
		}
	}
}
