using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;

namespace Frontier.Patches;

/// <summary>
/// BBCode가 요청하는 스프라이트 폰트 PNG 경로(<c>CARD_POOL∴…_energy_icon.png</c>)가 PCK 미탑재·유니코드 파일명 이슈 등으로 없을 때,
/// 베이스 게임의 아이언클래드 에너지 아이콘으로 로드한다.
/// </summary>
[HarmonyPatch]
internal static class FrontierShumitEnergySpriteFontAssetRedirectPatch
{
	/// <summary><c>GetAsset(string)</c> 비제네릭 오버로드만 패치(제네릭 <c>GetAsset&lt;TS&gt;</c>와의 AmbiguousMatch 방지).</summary>
	private static MethodBase TargetMethod()
	{
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		foreach (MethodInfo m in typeof(AssetCache).GetMethods(flags))
		{
			if (m.Name == nameof(AssetCache.GetAsset)
			    && !m.IsGenericMethodDefinition
			    && m.GetParameters().Length == 1
			    && m.GetParameters()[0].ParameterType == typeof(string))
			{
				return m;
			}
		}

		throw new InvalidOperationException("AssetCache.GetAsset(string) not found");
	}

	private const string ShumitSpriteFontEnergyPath =
		"res://images/packed/sprite_fonts/CARD_POOL\u2234FRONTIER-SHUMIT_CARD_POOL_energy_icon.png";

	private const string IroncladEnergyIconPath =
		"res://images/packed/sprite_fonts/ironclad_energy_icon.png";

	[HarmonyPrefix]
	private static void Prefix(ref string path)
	{
		if (path == ShumitSpriteFontEnergyPath)
		{
			path = IroncladEnergyIconPath;
		}
	}
}
