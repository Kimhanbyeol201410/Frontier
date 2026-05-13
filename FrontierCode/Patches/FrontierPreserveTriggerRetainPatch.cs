using System.Collections.Generic;
using System.Reflection;
using Frontier.Cards;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Patches;

/// <summary>
/// 「보존 발동」(<see cref="FrontierKeywords.PreserveTrigger"/>) 키워드를 가진 카드는
/// vanilla 「보존」(<see cref="MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Retain"/>) 키워드 없이도
/// 매 턴 종료 시 자동으로 손패에 유지되도록 한다.
///
/// <para>「보존 발동」의 한국어 키워드 설명에 이미 「턴 종료 시 버린 카드 더미로 가지 않으며…」가 포함되어 있어
/// 카드에 「보존」 키워드를 별도로 붙이면 동일 효과가 두 번 표기된다. 이 패치는 키워드 줄에서 「보존」 표시를
/// 제거하면서도 실제 retain 효과는 그대로 유지하기 위한 우회로다.</para>
/// </summary>
[HarmonyPatch(typeof(CardModel), "get_ShouldRetainThisTurn")]
internal static class FrontierPreserveTriggerRetainPatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        if (__instance.Keywords.Contains(FrontierKeywords.PreserveTrigger))
        {
            __result = true;
        }
    }
}

/// <summary>
/// 게임 본가의 <see cref="CardModel"/>는 <c>ShouldRetainThisTurn</c>이 true면 카드 설명 상단에 「보존.」 라벨을
/// 자동으로 끼워 넣는다 (<c>GetDescriptionForPile</c> 내부 <c>CardKeyword.Retain</c> 분기).
///
/// <para>「보존 발동」 카드는 자체 키워드 라벨로 이미 retain 의미를 전달하므로, vanilla 「보존.」 라벨을 함께
/// 표시하면 「보존 발동.\n보존.」이 중복으로 노출된다. 결과 문자열에서 retain 라벨 한 줄만 제거해 시각적 중복을
/// 해소하고, retain 동작 자체는 위쪽 <see cref="FrontierPreserveTriggerRetainPatch"/>가 유지한다.</para>
/// </summary>
[HarmonyPatch]
internal static class FrontierPreserveTriggerHideRetainLabelPatch
{
    /// <summary><c>CardKeywordExtensions.GetCardText(CardKeyword)</c>는 internal extension이라 우리 어셈블리에서
    /// 직접 호출이 불가능하다. reflection으로 정확히 같은 문자열을 얻어 결과 라벨 라인과 매칭한다.</summary>
    private static readonly MethodInfo? GetCardTextMethod = AccessTools.Method(
        "MegaCrit.Sts2.Core.Entities.Cards.CardKeywordExtensions:GetCardText",
        new[] { typeof(CardKeyword) });

    [HarmonyTargetMethod]
    private static MethodBase Target()
    {
        // GetDescriptionForPile 의 private 3-인자 오버로드를 패치한다. (PileType, DescriptionPreviewType, Creature)
        // DescriptionPreviewType 은 CardModel 내부 private nested enum이라 typeof 로 직접 참조할 수 없으므로
        // 메서드 이름과 매개변수 수로 식별한다. public 2-인자 / no-arg 오버로드는 결국 이 메서드를 호출한다.
        foreach (MethodInfo m in AccessTools.GetDeclaredMethods(typeof(CardModel)))
        {
            if (m.Name != "GetDescriptionForPile") { continue; }
            ParameterInfo[] ps = m.GetParameters();
            if (ps.Length == 3 && ps[0].ParameterType == typeof(PileType))
            {
                return m;
            }
        }

        throw new System.InvalidOperationException("CardModel.GetDescriptionForPile(PileType, …, Creature) 3-arg overload not found");
    }

    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref string __result)
    {
        if (string.IsNullOrEmpty(__result))
        {
            return;
        }

        if (!__instance.Keywords.Contains(FrontierKeywords.PreserveTrigger))
        {
            return;
        }

        string retainLabel = GetRetainLabel();
        if (string.IsNullOrEmpty(retainLabel))
        {
            return;
        }

        // retain 라벨이 그대로 한 줄로 들어있을 때만 그 줄만 제거한다. 다른 줄 텍스트에 우연히 포함되더라도
        // 분할 비교라 안전.
        string[] lines = __result.Split('\n');
        var filtered = new List<string>(lines.Length);
        bool removed = false;
        foreach (string line in lines)
        {
            if (!removed && line == retainLabel)
            {
                removed = true;
                continue;
            }
            filtered.Add(line);
        }

        if (removed)
        {
            __result = string.Join('\n', filtered);
        }
    }

    private static string GetRetainLabel()
    {
        if (GetCardTextMethod == null)
        {
            return string.Empty;
        }

        return GetCardTextMethod.Invoke(null, new object[] { CardKeyword.Retain }) as string ?? string.Empty;
    }
}
