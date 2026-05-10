using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;

namespace Frontier.Patches;

/// <summary>
/// BBCode가 요청하는 스프라이트 폰트 PNG 경로(<c>CARD_POOL∴…_energy_icon.png</c>)가 PCK 미탑재·유니코드 파일명 이슈 등으로 없을 때,
/// 베이스 게임의 아이언클래드 에너지 아이콘으로 로드한다.
/// </summary>
[HarmonyPatch(typeof(AssetCache), "GetAsset", new[] { typeof(string) })]
internal static class FrontierShumitEnergySpriteFontAssetRedirectPatch
{
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
