using Godot;

namespace Frontier.Localization;

/// <summary>
/// 모드 PNG가 <c>ResourceLoader</c>로는 로드되지 않을 때(임포트·PCK 미동기화) <c>FileAccess</c>로 바이너리를 읽어 텍스처를 만든다.
/// </summary>
internal static class FrontierResPngTexture
{
	/// <summary><c>ResourceLoader.Exists</c> 는 임포트된 리소스만 true — PCK 안의 raw PNG 는 <c>FileAccess</c> 로만 보인다.</summary>
	internal static bool ResFileExists(string resPath)
	{
		return !string.IsNullOrEmpty(resPath) && FileAccess.FileExists(resPath);
	}

	internal static Texture2D? TryLoadTexture2DFromRes(string resPath)
	{
		if (string.IsNullOrEmpty(resPath))
		{
			return null;
		}

		if (!FileAccess.FileExists(resPath))
		{
			return null;
		}

		using FileAccess? file = FileAccess.Open(resPath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			return null;
		}

		byte[] buffer = file.GetBuffer((long)file.GetLength());
		Image image = new();
		Error err = image.LoadPngFromBuffer(buffer);
		if (err != Error.Ok)
		{
			return null;
		}

		return ImageTexture.CreateFromImage(image);
	}
}
