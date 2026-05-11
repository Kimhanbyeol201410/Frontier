using System.Collections.Generic;
using System.Threading.Tasks;
using Frontier.Characters;
using Frontier.Powers;
using Frontier.Utilities;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart), typeof(IRunState), typeof(CombatState))]
internal static class FrontierCombatStartSessionPatch
{
	[HarmonyPrefix]
	private static void ResetCombatCounters()
	{
		FrontierSession.OnCombatStarted();
	}
}

/// <summary>
/// 플레이어마다 턴이 끝날 때 손패를 버리기 전에 호출된다(멀티에서 각 NetId별로 분리).
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeFlush))]
internal static class FrontierBeforeFlushSessionPatch
{
	[HarmonyPrefix]
	private static void ResetPerPlayerTurnCounters(CombatState combatState, Player player)
	{
		FrontierSession.OnPlayerBeforeHandFlush(player);
	}
}

[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Upgrade), typeof(IEnumerable<CardModel>), typeof(CardPreviewStyle))]
internal static class FrontierUpgradeCountPatch
{
	[HarmonyPostfix]
	private static void CountShumitUpgrades(IEnumerable<CardModel> cards)
	{
		HashSet<Player> owners = new();
		foreach (CardModel c in cards)
		{
			if (c?.Owner?.Character?.Id?.Entry == ShumitCharacter.CharacterId)
			{
				FrontierSession.RegisterUpgrade(c.Owner);
				owners.Add(c.Owner);
			}
		}

		foreach (Player o in owners)
		{
			if (FrontierSession.GetUpgradesThisCombat(o) >= 5 && FrontierSession.HasQueuedForgeBlueprint(o))
			{
				FrontierForgeBlueprintRewards.OfferAfterThresholdIfQueuedDeferred(o);
			}
		}
	}
}

[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Upgrade), typeof(IEnumerable<CardModel>), typeof(CardPreviewStyle))]
internal static class FrontierMasterPrideBlockOnUpgradePatch
{
	[HarmonyPrefix]
	private static void Prefix(IEnumerable<CardModel> cards, ref List<CardModel>? __state)
	{
		__state = new List<CardModel>();
		foreach (CardModel c in cards)
		{
			if (c != null && c.IsUpgradable)
			{
				__state.Add(c);
			}
		}
	}

	[HarmonyPostfix]
	private static void Postfix(ref List<CardModel>? __state)
	{
		if (__state == null || __state.Count == 0 || CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}

		List<CardModel> upgraded = new List<CardModel>(__state);
		Callable.From(() => MasterPrideGainBlockDeferred(upgraded)).CallDeferred();
	}

	private static async Task MasterPrideGainBlockDeferred(List<CardModel> upgraded)
	{
		foreach (CardModel c in upgraded)
		{
			Player? owner = c.Owner;
			if (owner?.Character?.Id?.Entry != ShumitCharacter.CharacterId)
			{
				continue;
			}

			ShumitMasterPridePower? p = owner.Creature.GetPower<ShumitMasterPridePower>();
			if (p == null || p.Amount <= 0m)
			{
				continue;
			}

			await CreatureCmd.GainBlock(owner.Creature, p.Amount, ValueProp.Move, null, fast: true);
		}
	}
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardExhausted))]
internal static class FrontierAfterCardExhaustedTrackPatch
{
	[HarmonyPrefix]
	private static void TrackBurnExhaust(CombatState combatState, PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card == null || card.Owner?.Character?.Id?.Entry != ShumitCharacter.CharacterId)
		{
			return;
		}

		if (card is Burn)
		{
			FrontierSession.RegisterBurnExhausted(card.Owner);
		}
	}
}
