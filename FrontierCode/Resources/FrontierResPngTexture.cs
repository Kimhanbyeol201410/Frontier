using Godot;

namespace Frontier.Resources;

/// <summary>
/// 모드 이미지가 <c>ResourceLoader</c>로는 로드되지 않을 때(임포트·PCK 미동기화) <c>FileAccess</c>로 바이너리를 읽어 텍스처를 만든다.
/// PNG 시그니처가 아니면 JPEG로 디코딩한다.
/// </summary>
internal static class FrontierResPngTexture
{
	/// <summary><c>ResourceLoader.Exists</c> 는 임포트된 리소스만 true — PCK 안의 raw 이미지는 <c>FileAccess</c> 로만 보인다.</summary>
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
		Error err = LoadImageFromBuffer(image, buffer);
		if (err != Error.Ok)
		{
			return null;
		}

		return ImageTexture.CreateFromImage(image);
	}

	private static Error LoadImageFromBuffer(Image image, byte[] buffer)
	{
		if (buffer.Length >= 8
		    && buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47
		    && buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A)
		{
			return image.LoadPngFromBuffer(buffer);
		}

		if (buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xD8)
		{
			return image.LoadJpgFromBuffer(buffer);
		}

		return image.LoadPngFromBuffer(buffer);
	}
}
