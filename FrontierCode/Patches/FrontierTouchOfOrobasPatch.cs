using System.Collections.Generic;
using Frontier.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Frontier.Patches;

/// <summary>
/// 오로바스의 손길(<see cref="TouchOfOrobas"/>) 변환 매핑 확장 — 슈미트의 시작 유물 <see cref="BrokenForgeRelic"/>(판단의 눈)
/// 을 <see cref="DivineEyeRelic"/>(신의 눈) 으로 변환한다.
///
/// <see cref="TouchOfOrobas"/> 는 private static get-only 프로퍼티 <c>RefinementUpgrades</c> 로
/// 매번 새 <see cref="Dictionary{TKey, TValue}"/> 를 생성한다. Postfix 에서 슈미트 엔트리만 추가하면
/// 매핑 미존재 시의 폴백(<c>Circlet</c>) 대신 정상적인 강화 유물로 교체된다.
/// </summary>
[HarmonyPatch(typeof(TouchOfOrobas), "get_RefinementUpgrades")]
internal static class FrontierTouchOfOrobasPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Dictionary<ModelId, RelicModel> __result)
    {
        __result[ModelDb.Relic<BrokenForgeRelic>().Id] = ModelDb.Relic<DivineEyeRelic>();
    }
}
