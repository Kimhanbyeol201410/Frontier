using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;

namespace Frontier.Patches;

/// <summary>
/// 모드 캐릭터(슈미트)는 게임 본가의 <c>AncientEventModel.DefineDialogues</c> 결과물에
/// 등록되어 있지 않다. 각 고대존재 이벤트의 <c>DefineDialogues</c> 반환값에
/// 슈미트 항목을 끼워넣어 <c>ancients.json</c>의 슈미트 대사 키들이 정상적으로 매칭되도록 한다.
///
/// <c>AncientDialogueSet.CharacterDialogues</c>는 init-only 프로퍼티이지만, 게임 본가에서
/// 항상 <see cref="Dictionary{TKey, TValue}"/> 인스턴스를 부여하므로 컬렉션 내부에 키를
/// 추가하는 것은 가능하다. 이후 게임이 <c>PopulateLocKeys</c>를 호출하면서 우리 항목까지
/// 자동으로 로컬라이즈 키를 세팅한다.
///
/// 키 패턴: <c>{ANCIENT_ID}.talk.FRONTIER-SHUMIT_CHARACTER.{visit}-{line}.{ancient|char|next}</c>
/// </summary>
internal static class FrontierAncientDialoguesPatch
{
    /// <summary>슈미트 캐릭터의 <c>characters</c> 테이블 키. <see cref="Character.ShumitCharacter.CharacterId"/>와 동일 값을 유지한다.</summary>
    private const string ShumitCharacterKey = "FRONTIER-SHUMIT_CHARACTER";

    /// <summary>visitIndex 0의 단순 2줄 대사를 가진 <see cref="AncientDialogue"/> 1개 배열. ancient → char 흐름.</summary>
    private static AncientDialogue[] BuildSimpleFirstVisit(string ancientSfx, string charSfx = "")
    {
        return new[]
        {
            new AncientDialogue(ancientSfx, charSfx) { VisitIndex = 0 },
        };
    }

    /// <summary>대상 set이 mutable <see cref="Dictionary{TKey, TValue}"/>이면 슈미트 키를 추가하고 PopulateLocKeys를 다시 호출한다.</summary>
    private static void Inject(AncientDialogueSet set, string ancientEntry, IReadOnlyList<AncientDialogue> dialogues)
    {
        if (set.CharacterDialogues is Dictionary<string, IReadOnlyList<AncientDialogue>> dict
            && !dict.ContainsKey(ShumitCharacterKey))
        {
            dict[ShumitCharacterKey] = dialogues;
            set.PopulateLocKeys(ancientEntry);
        }
    }

    [HarmonyPatch(typeof(Neow), "DefineDialogues")]
    internal static class NeowPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "NEOW", BuildSimpleFirstVisit("event:/sfx/npcs/neow/neow_welcome"));
        }
    }

    [HarmonyPatch(typeof(Darv), "DefineDialogues")]
    internal static class DarvPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "DARV", BuildSimpleFirstVisit("event:/sfx/npcs/darv/darv_introduction"));
        }
    }

    [HarmonyPatch(typeof(Pael), "DefineDialogues")]
    internal static class PaelPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "PAEL", BuildSimpleFirstVisit(""));
        }
    }

    [HarmonyPatch(typeof(Orobas), "DefineDialogues")]
    internal static class OrobasPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "OROBAS", BuildSimpleFirstVisit(""));
        }
    }

    [HarmonyPatch(typeof(Vakuu), "DefineDialogues")]
    internal static class VakuuPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "VAKUU", BuildSimpleFirstVisit(""));
        }
    }

    [HarmonyPatch(typeof(Tezcatara), "DefineDialogues")]
    internal static class TezcataraPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "TEZCATARA", BuildSimpleFirstVisit(""));
        }
    }

    [HarmonyPatch(typeof(Tanx), "DefineDialogues")]
    internal static class TanxPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "TANX", BuildSimpleFirstVisit(""));
        }
    }

    [HarmonyPatch(typeof(Nonupeipe), "DefineDialogues")]
    internal static class NonupeipePatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "NONUPEIPE", BuildSimpleFirstVisit(""));
        }
    }

    /// <summary>
    /// <see cref="TheArchitect.DefineDialogues"/>는 <c>private static</c>이므로 reflection으로 target을 지정한다.
    /// Architect는 visitIndex 0~3까지 채운다. visitIndex 0이 첫 클리어 엔딩 멘트(5줄).
    /// <c>EndAttackers</c>를 <see cref="ArchitectAttackers.Both"/>로 설정해 본가와 동일한 마무리 연출을 보여준다.
    /// </summary>
    [HarmonyPatch]
    internal static class TheArchitectPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase Target()
        {
            return typeof(TheArchitect).GetMethod(
                "DefineDialogues",
                BindingFlags.NonPublic | BindingFlags.Static);
        }

        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            var dialogues = new AncientDialogue[]
            {
                new AncientDialogue("", "", "", "", "")
                {
                    VisitIndex = 0,
                    EndAttackers = ArchitectAttackers.Both,
                },
                new AncientDialogue("", "")
                {
                    VisitIndex = 1,
                    EndAttackers = ArchitectAttackers.Both,
                },
                new AncientDialogue("", "")
                {
                    VisitIndex = 2,
                    EndAttackers = ArchitectAttackers.Both,
                },
                new AncientDialogue("", "")
                {
                    VisitIndex = 3,
                    EndAttackers = ArchitectAttackers.Both,
                },
            };

            Inject(__result, "THE_ARCHITECT", dialogues);
        }
    }
}
