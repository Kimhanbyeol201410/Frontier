using Frontier.Cards;
using HarmonyLib;
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
