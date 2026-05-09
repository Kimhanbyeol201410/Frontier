namespace Frontier.Localization;

/// <summary>
/// <c>sts2-frontier/localization/**/*.json</c> 값 후처리.
/// JSON 표준 <c>"줄1\n줄2"</c>는 역직렬화 시 이미 줄바꿈 문자가 되지만,
/// 일부 편집기·내보내기는 <c>\\n</c>(백슬래시 + n)이 문자열에 그대로 남기는 경우가 있어 게임에서 한 줄로 보인다.
/// </summary>
public static class FrontierLocJsonEscapes
{
	/// <summary>Frontier 모드 로캘 문자열에 공통 적용: 리터럴 이스케이프 시퀀스를 실제 제어 문자로 치환한다.</summary>
	public static string Normalize(string? raw)
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
