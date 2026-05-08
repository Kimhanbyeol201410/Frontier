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
        return $"res://mods-unpacked/Frontier/images/packed/card_portraits/shumit/{filename}";
    }

    public static string BigCardImagePath(this string filename)
    {
        return $"res://mods-unpacked/Frontier/images/cards/{filename}";
    }
}
