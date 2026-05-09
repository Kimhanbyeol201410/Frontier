using Frontier.Cards;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Patches;

/// <summary>
/// BaseLib <see cref="BaseLib.Abstracts.CustomCardPortrait"/> 가 <c>ResourceLoader.Load</c> 만 사용해
/// 임포트 없는 모드 PNG 에서 실패한다. 슈미트 카드는 <see cref="ShumitCard.CustomPortrait"/>(버퍼 로드)를 우선한다.
/// </summary>
// 프로퍼티 getter는 이름을 "Portrait" 로 지정해야 한다. "get_Portrait" 는 대상 미해결(HarmonyException)이 난다.
[HarmonyPatch(typeof(CardModel), nameof(CardModel.Portrait), MethodType.Getter)]
[HarmonyBefore("BaseLib.Abstracts.CustomCardPortrait")]
internal static class FrontierShumitCardPortraitBufferPatch
{
	[HarmonyPrefix]
	private static bool TryBufferPortraitFirst(CardModel __instance, ref Texture2D? __result)
	{
		if (__instance is not ShumitCard sc)
		{
			return true;
		}

		Texture2D? fromProperty = sc.CustomPortrait;
		if (fromProperty != null)
		{
			__result = fromProperty;
			return false;
		}

		return true;
	}
}
