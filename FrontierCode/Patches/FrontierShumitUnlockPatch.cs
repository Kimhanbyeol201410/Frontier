using System.Collections.Generic;
using System.Linq;
using Frontier.Characters;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace Frontier.Patches;

/// <summary>
/// 슈미트 캐릭터 해금 조건 패치.
///
/// 게임의 <see cref="UnlockState.Characters"/> getter 는 <see cref="ModelDb.AllCharacters"/> 전체를 모은 뒤
/// 특정 Epoch 가 풀리지 않은 캐릭터만 제거하는 구조다. 이 때문에 <see cref="ModelDb"/> 에 추가된 모든
/// 모드 캐릭터는 자동으로 "해금됨" 으로 인식된다.
/// (참고: <c>CharacterModel.UnlocksAfterRunAs</c> 는 잠금 안내 문구의 <c>{Prerequisite}</c> 변수에만 쓰인다.)
///
/// 슈미트는 디펙트 클리어를 해금 조건으로 두기로 했으므로,
/// <c>Defect1Epoch</c> 가 Revealed 상태가 아닐 때 결과 목록에서 슈미트를 제거한다.
/// </summary>
[HarmonyPatch(typeof(UnlockState), "get_Characters")]
internal static class FrontierShumitUnlockPatch
{
    [HarmonyPostfix]
    private static void Postfix(UnlockState __instance, ref IEnumerable<CharacterModel> __result)
    {
        if (__instance.IsEpochRevealed<Defect1Epoch>())
        {
            return;
        }

        ShumitCharacter shumit = ModelDb.Character<ShumitCharacter>();
        __result = __result.Where(c => c != shumit).ToList();
    }
}
