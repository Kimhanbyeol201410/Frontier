using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace Frontier.Patches;

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
		if (__result == null || __result.Count == 0 || !FrontierModLocJsonEscapes.IsFrontierLocalizationPath(path))
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

			__result[key] = FrontierModLocJsonEscapes.Normalize(v);
		}
	}
}

/// <summary>
/// <c>sts2-frontier/localization/**/*.json</c> 값 후처리.
/// </summary>
internal static class FrontierModLocJsonEscapes
{
	internal static string Normalize(string? raw)
	{
		if (string.IsNullOrEmpty(raw))
		{
			return raw ?? "";
		}

		string s = raw;
		if (s.IndexOf('\\') < 0)
		{
			return s;
		}

		return s
			.Replace("\\r\\n", "\n")
			.Replace("\\n", "\n")
			.Replace("\\r", "\n")
			.Replace("\\t", "\t");
	}

	internal static bool IsFrontierLocalizationPath(string? path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}

		string p = path.Replace('\\', '/');
		return p.Contains("sts2-frontier/", System.StringComparison.OrdinalIgnoreCase)
		       && p.Contains("/localization/", System.StringComparison.OrdinalIgnoreCase);
	}
}
