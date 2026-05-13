using System.Collections.Generic;
using System.Linq;
using Frontier.Cards;
using Frontier.Characters;
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
///
/// <para>슈미트는 «무수히 많은 기억»이 Ancient 카드이지만 <see cref="ShumitCardPool.FilterThroughEpochs"/>
/// 에서 제외되어 후보 검색이 비어버린다. 슈미트 전용 분기로 «무수히 많은 기억»을 직접 부여한다.
/// 그 외 빈 풀은 베이스 <see cref="Apotheosis"/>로 대체.</para>
///
/// <para>료슈 모드 프리픽스는 료슈일 때만 처리하므로, <c>HarmonyAfter</c>로 그 뒤에 실행된다.</para>
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

		// 슈미트 전용: «무수히 많은 기억» 을 직접 부여한다(풀에서 제외되어 있어 일반 검색은 빈 리스트).
		// Character 인스턴스가 ModelDb 복제로 다른 객체일 가능성에 대비해 CharacterId 도 폴백 체크.
		if (player.Character is ShumitCharacter || player.Character?.Id.Entry == ShumitCharacter.CharacterId)
		{
			__instance.AncientCard = ModelDb.Card<CountlessMemoriesCard>().Id;
			return false;
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

/// <summary>
/// 콘솔(<c>relic frontier-...</c> 등)로 <see cref="DustyTome"/> 를 직접 부여한 경우
/// <see cref="DustyTome.SetupForPlayer"/> 가 호출되지 않아 <c>AncientCard</c> 가 <c>null</c> 상태로
/// <see cref="DustyTome.AfterObtained"/> 가 실행되어 <c>ModelDb.GetById&lt;CardModel&gt;(null)</c> 에서
/// <see cref="System.ArgumentNullException"/> 이 발생한다.
/// 안전망으로 여기서 자동으로 <see cref="DustyTome.SetupForPlayer"/> 를 호출해 부여한다.
/// </summary>
[HarmonyPatch(typeof(DustyTome), nameof(DustyTome.AfterObtained))]
internal static class DustyTomeAfterObtainedAutoSetupPatch
{
	[HarmonyPrefix]
	private static void Prefix(DustyTome __instance)
	{
		if (__instance.AncientCard != null)
		{
			return;
		}

		Player? player = __instance.Owner;
		if (player != null)
		{
			__instance.SetupForPlayer(player);
		}
	}
}
