using Frontier.Characters;
using Frontier.Utilities;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

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

[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Upgrade), typeof(System.Collections.Generic.IEnumerable<CardModel>), typeof(MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle))]
internal static class FrontierUpgradeCountPatch
{
	[HarmonyPostfix]
	private static void CountShumitUpgrades(System.Collections.Generic.IEnumerable<CardModel> cards)
	{
		foreach (CardModel c in cards)
		{
			if (c?.Owner?.Character?.Id?.Entry == ShumitCharacter.CharacterId)
			{
				FrontierSession.RegisterUpgrade(c.Owner);
			}
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
