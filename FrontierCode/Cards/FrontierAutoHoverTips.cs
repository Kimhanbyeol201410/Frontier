using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Frontier.Cards;

/// <summary>
/// 카드 설명(<see cref="CardModel.Description"/>) raw 텍스트에 등장하는 <c>[gold]키워드[/gold]</c> 토큰을 검사하여
/// 매칭되는 vanilla/모드 키워드·파워·상태 카드 호버 설명을 자동으로 생성한다.
///
/// <para>본가 카드는 <c>ExtraHoverTips</c>에 매번 명시적으로 <see cref="HoverTipFactory"/> 호출을 추가하지만, 모드
/// 카드들은 description 안에 키워드가 보이는데 호버 설명이 누락되는 경우가 잦다. <see cref="ShumitCard"/> 가 이 클래스를
/// 일괄 사용해 자동으로 호버를 채워주도록 한다.</para>
///
/// <para>매칭은 <c>[gold]...[/gold]</c> 태그가 닫힌 정확한 토큰만 비교하므로, DynamicVar 변수명(예: <c>HeatPerTurn</c>)이
/// raw 텍스트에 포함되어도 오탐하지 않는다. 동일 매핑 인덱스는 한 번만 yield 한다.</para>
/// </summary>
internal static class FrontierAutoHoverTips
{
    private delegate IHoverTip TipFactory();

    private sealed record Mapping(string[] Tokens, TipFactory Factory);

    /// <summary>한국어/영어 description 모두를 지원하기 위해 각 항목에 양쪽 토큰을 모두 등록한다.</summary>
    private static readonly Mapping[] _mappings = new[]
    {
        // === Vanilla CardKeyword 호버 ===
        new Mapping(new[] { "[gold]소멸[/gold]", "[gold]Exhaust[/gold]" },
            () => HoverTipFactory.FromKeyword(CardKeyword.Exhaust)),
        new Mapping(new[] { "[gold]보존[/gold]", "[gold]Retain[/gold]" },
            () => HoverTipFactory.FromKeyword(CardKeyword.Retain)),
        new Mapping(new[] { "[gold]휘발성[/gold]", "[gold]Ethereal[/gold]" },
            () => HoverTipFactory.FromKeyword(CardKeyword.Ethereal)),
        new Mapping(new[] { "[gold]선천성[/gold]", "[gold]Innate[/gold]" },
            () => HoverTipFactory.FromKeyword(CardKeyword.Innate)),
        new Mapping(new[] { "[gold]사용불가[/gold]", "[gold]사용 불가[/gold]", "[gold]Unplayable[/gold]" },
            () => HoverTipFactory.FromKeyword(CardKeyword.Unplayable)),
        new Mapping(new[] { "[gold]교활[/gold]", "[gold]Sly[/gold]" },
            () => HoverTipFactory.FromKeyword(CardKeyword.Sly)),
        new Mapping(new[] { "[gold]영구[/gold]", "[gold]Eternal[/gold]" },
            () => HoverTipFactory.FromKeyword(CardKeyword.Eternal)),

        // === Vanilla Power 호버 ===
        new Mapping(new[] { "[gold]취약[/gold]", "[gold]Vulnerable[/gold]" },
            () => HoverTipFactory.FromPower<VulnerablePower>()),
        new Mapping(new[] { "[gold]약화[/gold]", "[gold]Weak[/gold]" },
            () => HoverTipFactory.FromPower<WeakPower>()),
        new Mapping(new[] { "[gold]힘[/gold]", "[gold]Strength[/gold]" },
            () => HoverTipFactory.FromPower<StrengthPower>()),
        new Mapping(new[] { "[gold]민첩[/gold]", "[gold]Dexterity[/gold]" },
            () => HoverTipFactory.FromPower<DexterityPower>()),
        new Mapping(new[] { "[gold]활력[/gold]", "[gold]Vigor[/gold]" },
            () => HoverTipFactory.FromPower<VigorPower>()),
        new Mapping(new[] { "[gold]손상[/gold]", "[gold]Frail[/gold]" },
            () => HoverTipFactory.FromPower<FrailPower>()),
        new Mapping(new[] { "[gold]무형[/gold]", "[gold]Intangible[/gold]" },
            () => HoverTipFactory.FromPower<IntangiblePower>()),

        // === Vanilla 상태 카드 호버 ===
        new Mapping(new[] { "[gold]화상[/gold]", "[gold]Burn[/gold]" },
            () => HoverTipFactory.FromCard<Burn>()),
        new Mapping(new[] { "[gold]부상[/gold]", "[gold]Wound[/gold]" },
            () => HoverTipFactory.FromCard<Wound>()),
        new Mapping(new[] { "[gold]어지러움[/gold]", "[gold]Dazed[/gold]" },
            () => HoverTipFactory.FromCard<Dazed>()),
        new Mapping(new[] { "[gold]점액투성이[/gold]", "[gold]점액[/gold]", "[gold]Slimed[/gold]" },
            () => HoverTipFactory.FromCard<Slimed>()),

        // === 모드 키워드 호버 ===
        new Mapping(new[] { "[gold]열기[/gold]", "[gold]Heat[/gold]" },
            () => HoverTipFactory.FromKeyword(FrontierKeywords.Heat)),
        new Mapping(new[] { "[gold]신체 화상[/gold]", "[gold]신체화상[/gold]", "[gold]Body Burn[/gold]", "[gold]BodyBurn[/gold]" },
            () => HoverTipFactory.FromKeyword(FrontierKeywords.BodyBurn)),
        new Mapping(new[] { "[gold]재련[/gold]", "[gold]Reforge[/gold]" },
            () => HoverTipFactory.FromKeyword(FrontierKeywords.Reforge)),
        new Mapping(new[] { "[gold]걸작[/gold]", "[gold]Masterpiece[/gold]" },
            () => HoverTipFactory.FromKeyword(FrontierKeywords.Masterpiece)),
        new Mapping(new[] { "[gold]인챈트[/gold]", "[gold]Enchant[/gold]" },
            () => HoverTipFactory.FromKeyword(FrontierKeywords.Enchant)),
        new Mapping(new[] { "[gold]열화[/gold]", "[gold]Thermal Degradation[/gold]", "[gold]ThermalDegradation[/gold]" },
            () => HoverTipFactory.FromKeyword(FrontierKeywords.ThermalDegradation)),
        new Mapping(new[] { "[gold]반전[/gold]", "[gold]Invert[/gold]" },
            () => HoverTipFactory.FromKeyword(FrontierKeywords.Invert)),
        new Mapping(new[] { "[gold]보존 발동[/gold]", "[gold]Preserve Trigger[/gold]", "[gold]PreserveTrigger[/gold]" },
            () => HoverTipFactory.FromKeyword(FrontierKeywords.PreserveTrigger)),
        new Mapping(new[] { "[gold]강화 불가[/gold]", "[gold]Unupgradable[/gold]" },
            () => HoverTipFactory.FromKeyword(FrontierKeywords.Unupgradable)),
    };

    /// <summary>
    /// 카드 설명을 읽어 매핑 테이블과 비교하고, 발견된 키워드들의 호버 설명을 <see cref="IHoverTip"/> 시퀀스로 반환한다.
    /// </summary>
    public static IEnumerable<IHoverTip> CollectFor(CardModel card)
    {
        string text;
        try
        {
            text = card.Description?.GetRawText() ?? string.Empty;
        }
        catch
        {
            yield break;
        }

        if (text.Length == 0)
        {
            yield break;
        }

        foreach (Mapping mapping in _mappings)
        {
            if (!ContainsAnyToken(text, mapping.Tokens))
            {
                continue;
            }

            IHoverTip? tip;
            try
            {
                tip = mapping.Factory();
            }
            catch
            {
                // 모델 초기화 순서 등으로 팩토리 호출이 실패할 수 있다. 한 항목 실패가 다른 호버를 막지 않도록 무시.
                tip = null;
            }

            if (tip != null)
            {
                yield return tip;
            }
        }
    }

    private static bool ContainsAnyToken(string haystack, string[] tokens)
    {
        foreach (string token in tokens)
        {
            if (haystack.IndexOf(token, StringComparison.Ordinal) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
