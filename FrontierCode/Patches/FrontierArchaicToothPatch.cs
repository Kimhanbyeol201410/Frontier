using System.Collections.Generic;
using Frontier.Cards;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Frontier.Patches;

/// <summary>
/// 고대의 이빨(<see cref="ArchaicTooth"/>) 변환 매핑 확장 — 슈미트의 시작 카드 <see cref="ForgingCard"/>(단조) 가
/// 덱에 있으면 <see cref="AncientForgingCard"/>(고대의 단조) 로 변환되도록 한다.
///
/// <see cref="ArchaicTooth"/> 는 private static get-only 프로퍼티 <c>TranscendenceUpgrades</c> 로
/// 매번 새 <see cref="Dictionary{TKey, TValue}"/> 를 생성한다. Postfix 에서 슈미트 엔트리만 추가하면
/// <c>GetTranscendenceStarterCard</c> 와 <c>GetTranscendenceTransformedCard</c> 모두에서 자동으로 인식된다.
///
/// 결과로 <see cref="ArchaicTooth.TranscendenceCards"/> 정적 프로퍼티에도
/// <see cref="AncientForgingCard"/> 가 자동 포함되어 <c>DustyTome</c> 등에서도 함께 처리된다.
/// </summary>
[HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
internal static class FrontierArchaicToothPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<ForgingCard>().Id] = ModelDb.Card<AncientForgingCard>();
    }
}
