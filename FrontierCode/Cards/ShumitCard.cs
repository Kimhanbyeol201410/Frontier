using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using Frontier.Extensions;
using Frontier.Resources;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Cards;

public abstract class ShumitCard : CustomCardModel
{
    private const string PlaceholderPortraitFile = "placeholder.png";

    protected ShumitCard(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true)
        : base(cost, type, rarity, target, showInCardLibrary)
    {
    }

    /// <summary>Exhaust, Retain 등. <see cref="FrontierKeywords.Heat"/>는 <c>AutoKeywordPosition.None</c>이라 하단 키워드 줄에는 붙지 않고, 카드 호버 시 열기 설명 툴팁만 제공한다.</summary>
    protected virtual IEnumerable<CardKeyword> ShumitCanonicalKeywords => Enumerable.Empty<CardKeyword>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => ShumitCanonicalKeywords.Append(FrontierKeywords.Heat);

    /// <summary>카드별 PNG가 없으면 <c>images/packed/card_portraits/shumit/placeholder.png</c>를 쓴다.</summary>
    public override string PortraitPath => ResolvePortraitPath($"{Id.Entry.RemoveFrontierPrefix().ToLowerInvariant()}.png".CardImagePath());

    /// <summary>카드별 대형 일러스트가 없으면 <c>images/cards/placeholder.png</c>를 쓴다.</summary>
    public override string CustomPortraitPath => ResolvePortraitPath($"{Id.Entry.RemoveFrontierPrefix().ToLowerInvariant()}.png".BigCardImagePath());

    /// <summary>손패/라이브러리 등 <see cref="CardModel.Portrait"/> — <c>CustomCardPortrait</c> 가 <c>ResourceLoader</c> 를 쓰기 전에 버퍼 로드.</summary>
    public override Texture2D? CustomPortrait => _cachedPortrait ??= LoadPortraitFromPckOrNull();

    private Texture2D? _cachedPortrait;

    private Texture2D? LoadPortraitFromPckOrNull()
    {
        string path = ResolvePortraitPath($"{Id.Entry.RemoveFrontierPrefix().ToLowerInvariant()}.png".BigCardImagePath());
        if (path == CardModel.MissingPortraitPath)
        {
            return null;
        }

        return FrontierResPngTexture.TryLoadTexture2DFromRes(path);
    }

    private static bool PathExistsOnFilesystem(string path)
    {
        return ResourceLoader.Exists(path) || FrontierResPngTexture.ResFileExists(path);
    }

    private static string ResolvePortraitPath(string specificPath)
    {
        if (PathExistsOnFilesystem(specificPath))
        {
            return specificPath;
        }

        string modPackedFallback = specificPath.Contains("/card_portraits/", System.StringComparison.Ordinal)
            ? PlaceholderPortraitFile.CardImagePath()
            : PlaceholderPortraitFile.BigCardImagePath();

        if (PathExistsOnFilesystem(modPackedFallback))
        {
            return modPackedFallback;
        }

        return CardModel.MissingPortraitPath;
    }
}


