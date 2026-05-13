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

    /// <summary>Exhaust, Retain 등 카드 자체 특성으로서의 키워드. 카드 라벨/effect 동작과 직결되므로 여기에 명시.</summary>
    protected virtual IEnumerable<CardKeyword> ShumitCanonicalKeywords => Enumerable.Empty<CardKeyword>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => ShumitCanonicalKeywords;

    /// <summary>
    /// 카드 설명에 등장하는 모든 키워드/파워/상태 카드의 호버 설명을 <see cref="FrontierAutoHoverTips"/>가
    /// 자동으로 감지해 노출한다. 슈미트 카드뿐 아니라 «열기/신체 화상/취약/약화/힘/민첩/활력/화상/재련»…
    /// 등 description 안 <c>[gold]...[/gold]</c> 토큰에 매칭되는 모든 호버가 한 번에 채워진다.
    /// </summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (IHoverTip baseTip in base.ExtraHoverTips)
            {
                yield return baseTip;
            }

            foreach (IHoverTip auto in FrontierAutoHoverTips.CollectFor(this))
            {
                yield return auto;
            }
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


