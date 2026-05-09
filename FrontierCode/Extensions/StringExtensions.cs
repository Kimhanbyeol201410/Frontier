namespace Frontier.Extensions;

public static class StringExtensions
{
    public static string RemoveFrontierPrefix(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Replace("FRONTIER-", "");
    }

    public static string CardImagePath(this string filename)
    {
        return $"res://sts2-frontier/images/packed/card_portraits/shumit/{filename}";
    }

    public static string BigCardImagePath(this string filename)
    {
        return $"res://sts2-frontier/images/cards/{filename}";
    }

    /// <summary>Soldoros <c>CharacterUiPath</c> 와 동일: <c>res://sts2-frontier/images/charui/{filename}</c>.</summary>
    public static string CharacterUiPath(this string filename)
    {
        return $"res://sts2-frontier/images/charui/{filename}";
    }

    /// <summary>캐릭터 선택 배경 등 — <c>res://sts2-frontier/scenes/{relativePath}</c>.</summary>
    public static string FrontierScenePath(this string relativePath)
    {
        return $"res://sts2-frontier/scenes/{relativePath}";
    }
}

