using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace Frontier.Localization;

/// <summary>
/// 로캘 테이블이 로드된 직후(및 언어 전환 시) 내장 문자열을 합친다.
/// <c>LocManager</c> 초기화가 <c>ModelDb.Init</c>보다 앞서므로 ModelLocPatch만으로는 첫 조회에 키가 없을 수 있다.
/// </summary>
[HarmonyPatch(typeof(LocManager), "SetLanguageInternal")]
internal static class FrontierLocMergePatch
{
	[HarmonyPostfix]
	private static void MergeFrontierEntries(LocManager __instance, string language)
	{
		FrontierEmbeddedLoc.ApplyToLocManager(__instance, language);
	}
}
