namespace Frontier;

/// <summary>
/// DLL-only 배포에서는 <c>res://sts2-frontier/...</c> 가 마운트되지 않을 수 있다.
/// 유물 아이콘 오버라이드는 베이스 게임에 항상 있는 PNG를 쓴다.
/// </summary>
public static class FrontierAssetPaths
{
	/// <summary>베이스 게임 전용 경로. PCK 없이도 <see cref="Godot.ResourceLoader"/> 가 로드한다.</summary>
	public const string VanillaRelicFallbackPng = "res://images/powers/missing_power.png";
}
