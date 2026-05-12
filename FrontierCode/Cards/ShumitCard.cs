using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using Frontier.Extensions;
using Frontier.Relics;
using Frontier.Resources;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
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

    /// <summary>모든 슈미트 카드는 «열기»를 다루므로 호버 시 «열기»와 «신체 화상» 키워드 설명을 함께 노출한다. «열기» 자체는 <see cref="CanonicalKeywords"/> 가 처리하므로 여기서는 «신체 화상» 만 추가.</summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (IHoverTip baseTip in base.ExtraHoverTips)
            {
                yield return baseTip;
            }

            yield return HoverTipFactory.FromKeyword(FrontierKeywords.BodyBurn);
        }
    }

    /// <summary>카드별 PNG가 없으면 <c>images/packed/card_portraits/shumit/placeholder.png</c>를 쓴다.</summary>
    public override string PortraitPath => ResolvePortraitPath($"{Id.Entry.RemoveFrontierPrefix().ToLowerInvariant()}.png".CardImagePath());

    /// <summary>카드별 대형 일러스트가 없으면 <c>images/cards/placeholder.png</c>를 쓴다.</summary>
    public override string CustomPortraitPath => ResolvePortraitPath($"{Id.Entry.RemoveFrontierPrefix().ToLowerInvariant()}.png".BigCardImagePath());

    /// <summary>손패/라이브러리 등 <see cref="CardModel.Portrait"/> — <c>CustomCardPortrait</c> 가 <c>ResourceLoader</c> 를 쓰기 전에 버퍼 로드.</summary>
    public override Texture2D? CustomPortrait => _cachedPortrait ??= LoadPortraitFromPckOrNull();

    private Texture2D? _cachedPortrait;

    /// <summary>카드가 새로 생성되어 슈미트 덱·전투 더미에 추가될 때 호출. «신의 눈» 보유 시 «걸작» 카드의 <c>MasterpieceLeft</c> 표시값을 -5 동기화한다.</summary>
    public override void AfterCreated()
    {
        base.AfterCreated();
        DivineEyeRelic.ApplyMasterpieceReductionIfApplicable(this);
    }

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


