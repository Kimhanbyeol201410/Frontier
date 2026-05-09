using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace Frontier.Localization;

/// <summary>
/// 모드 JSON이 <see cref="LocManager.LoadTable"/> 로 읽힌 뒤, 리터럴 <c>\n</c> 등을 정규화한다.
/// BaseLib <c>SimpleLoc</c> 포스트픽스 뒤에 실행해 단축 표기 변환 결과에도 치환이 적용되게 한다.
/// </summary>
[HarmonyPatch(typeof(LocManager), "LoadTable")]
[HarmonyAfter("BaseLib.Patches.Localization.SimpleLoc")]
internal static class FrontierLocLoadTablePatch
{
	[HarmonyPostfix]
	private static void NormalizeFrontierJsonEscapes(string path, Dictionary<string, string>? __result)
	{
		if (__result == null || __result.Count == 0 || !FrontierLocJsonEscapes.IsFrontierLocalizationPath(path))
		{
			return;
		}

		List<string> keys = new(__result.Keys);
		foreach (string key in keys)
		{
			string v = __result[key];
			if (string.IsNullOrEmpty(v))
			{
				continue;
			}

			__result[key] = FrontierLocJsonEscapes.Normalize(v);
		}
	}
}
