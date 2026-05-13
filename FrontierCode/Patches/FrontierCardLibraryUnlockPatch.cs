using System.Collections.Generic;
using System.Reflection;
using Frontier.Characters;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace Frontier.Patches;

/// <summary>
/// 백과사전(Card Library)에서 <see cref="ShumitCardPool"/>에 속한 모든 카드를 잠금 해제된 것으로 표시한다.
///
/// 본체 <see cref="NCardLibraryGrid.GetCardVisibility"/>는 다음 규칙으로 카드 상태를 결정한다:
///   1) <c>_unlockedCards</c>에 없으면 <see cref="MegaCrit.Sts2.Core.Entities.UI.ModelVisibility.Locked"/> (자물쇠)
///   2) <c>Progress.DiscoveredCards</c>에 없으면 <see cref="MegaCrit.Sts2.Core.Entities.UI.ModelVisibility.NotSeen"/> ("?" 흐릿)
///   3) 둘 다 만족하면 <see cref="MegaCrit.Sts2.Core.Entities.UI.ModelVisibility.Visible"/>
///
/// <see cref="ShumitCardPool.FilterThroughEpochs"/>가 시작 카드/인챈트/변환 전용 카드(걸작·고대의 이빨·먼지 쌓인 책)
/// 등을 풀에서 제외하므로, 백과사전의 <c>_unlockedCards</c>(= <c>GetUnlockedCards</c> 결과 집합)에 이들이 빠져
/// 자물쇠로 표시된다. 그러나 백과사전에는 보여주는 편이 자연스럽다.
///
/// 보상·상점·변환 풀은 여전히 <c>GetUnlockedCards</c>를 거치므로 기존 필터가 그대로 적용된다.
/// 미구현 «불사르지 않는 몸»은 <see cref="ShumitCardPool.GenerateAllCards"/>에 포함되지 않으므로 자동 숨김.
/// </summary>
[HarmonyPatch(typeof(NCardLibraryGrid), "RefreshVisibility")]
internal static class FrontierCardLibraryUnlockPatch
{
    private static readonly FieldInfo? UnlockedCardsField =
        AccessTools.Field(typeof(NCardLibraryGrid), "_unlockedCards");

    [HarmonyPostfix]
    private static void Postfix(NCardLibraryGrid __instance)
    {
        if (UnlockedCardsField == null)
        {
            return;
        }

        if (UnlockedCardsField.GetValue(__instance) is not HashSet<CardModel> unlocked)
        {
            return;
        }

        ShumitCardPool pool = ModelDb.CardPool<ShumitCardPool>();
        foreach (CardModel c in pool.AllCards)
        {
            unlocked.Add(c);
        }
    }
}
