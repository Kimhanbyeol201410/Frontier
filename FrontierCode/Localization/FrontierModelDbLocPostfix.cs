using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Localization;

/// <summary>
/// <see cref="ModelDb.Init"/> 직후 한 번 더 병합한다. 일부 초기화 순서에서 <c>relics</c> 테이블이 아직 비어 있거나
/// <see cref="LocManager.SetLanguageInternal"/> 포스트픽스만으로는 키가 안 잡히는 경우를 줄인다.
/// </summary>
[HarmonyPatch(typeof(ModelDb), "Init")]
internal static class FrontierModelDbLocPostfix
{
	[HarmonyPostfix]
	private static void MergeEmbeddedLocAfterModels()
	{
		LocManager? loc = LocManager.Instance;
		if (loc == null || string.IsNullOrEmpty(loc.Language))
		{
			return;
		}

		FrontierEmbeddedLoc.ApplyToLocManager(loc, loc.Language);
	}
}
