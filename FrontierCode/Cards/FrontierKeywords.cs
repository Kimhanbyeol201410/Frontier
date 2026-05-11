using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Frontier.Cards;

public static class FrontierKeywords
{
    [CustomEnum(null)]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Heat;

    [CustomEnum(null)]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword BodyBurn;

    [CustomEnum(null)]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Reforge;

    [CustomEnum(null)]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Masterpiece;

    [CustomEnum(null)]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Enchant;

    [CustomEnum(null)]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword ThermalDegradation;
}
