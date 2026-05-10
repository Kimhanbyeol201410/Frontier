using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Frontier.Patches;

/// <summary>
/// Darv 이벤트의 <see cref="DustyTome"/> 옵션에서 <see cref="DustyTome.SetupForPlayer"/>가
/// Ancient 희귀 카드 후보를 뽑는데, 모드 전용 카드풀에 해당 카드가 없으면
/// <c>NextItem</c>이 null을 돌려 <c>.Id</c>에서 NRE가 난다. (슈미트 등)
/// 료슈 모드 프리픽스는 료슈일 때만 처리하므로, 그 외 빈 풀은 여기서 베이스 Ancient로 대체한다.
/// </summary>
[HarmonyPatch(typeof(DustyTome), nameof(DustyTome.SetupForPlayer))]
[HarmonyAfter("boninall.ryoshu")]
internal static class DustyTomeEmptyAncientPoolPatch
{
	[HarmonyPrefix]
	private static bool Prefix(DustyTome __instance, Player player)
	{
		if (player?.Character?.CardPool == null || player.RunState == null)
		{
			return true;
		}

		List<CardModel> candidates = (from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
			where c.Rarity == CardRarity.Ancient && !ArchaicTooth.TranscendenceCards.Contains(c)
			select c).ToList();

		if (candidates.Count > 0)
		{
			return true;
		}

		__instance.AncientCard = ModelDb.Card<Apotheosis>().Id;
		return false;
	}
}
