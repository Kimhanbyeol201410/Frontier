using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
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
    internal const string ShumitCharacterKey = "FRONTIER-SHUMIT_CHARACTER";

    /// <summary>
    /// <see cref="AncientDialogueSet"/> 인스턴스마다 "슈미트가 이 ancient를 진짜 처음 마주쳤을 때" 사용할
    /// 별도 dialogue 객체를 매핑한다. 본가의 <see cref="AncientDialogueSet.FirstVisitEverDialogue"/>는
    /// 캐릭터 무관하게 발동되므로, 슈미트 한정 분기를 위해 별도 저장소를 둔다.
    /// </summary>
    internal static readonly ConditionalWeakTable<AncientDialogueSet, AncientDialogue> ShumitFirstMeetings = new();

    /// <summary>각 줄이 2-line(ancient → char) 인 dialogue 4세트 (visitIndex 0~3) 생성. SFX 4종이 그대로 visitIndex 0~3에 매핑된다.</summary>
    private static AncientDialogue[] BuildShumitDialogues(string sfx0, string sfx1, string sfx2, string sfx3)
    {
        return new[]
        {
            new AncientDialogue(sfx0, "") { VisitIndex = 0 },
            new AncientDialogue(sfx1, "") { VisitIndex = 1 },
            new AncientDialogue(sfx2, "") { VisitIndex = 2 },
            new AncientDialogue(sfx3, "") { VisitIndex = 3 },
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

    /// <summary>
    /// 슈미트 전용 "최초의 만남" dialogue를 만들어 <paramref name="set"/>에 매핑한다.
    /// charEntry 슬롯을 <c>FRONTIER-SHUMIT_CHARACTER.firstMeeting</c>으로 넘겨
    /// <c>{ancient}.talk.FRONTIER-SHUMIT_CHARACTER.firstMeeting.0-N.{ancient|char|next}</c> 키 패턴을
    /// 본가 <see cref="AncientDialogue.PopulateLines"/>가 자동 매핑하게 한다.
    /// </summary>
    private static void InjectFirstMeeting(AncientDialogueSet set, string ancientEntry, int lineCount, string sfx)
    {
        if (ShumitFirstMeetings.TryGetValue(set, out _)) { return; }

        var sfxPaths = new string[lineCount];
        for (int i = 0; i < lineCount; i++) { sfxPaths[i] = (i == 0) ? sfx : ""; }

        var dialogue = new AncientDialogue(sfxPaths);
        dialogue.PopulateLines(ancientEntry, $"{ShumitCharacterKey}.firstMeeting", 0);

        for (int i = 0; i < dialogue.Lines.Count - 1; i++)
        {
            MegaCrit.Sts2.Core.Localization.LocString? lineText = dialogue.Lines[i].LineText;
            if (lineText == null) { continue; }
            string locEntryKey = lineText.LocEntryKey;
            string baseKey = locEntryKey.Substring(0, locEntryKey.LastIndexOf('.'));
            dialogue.Lines[i].NextButtonText = new MegaCrit.Sts2.Core.Localization.LocString("ancients", baseKey + ".next");
        }

        ShumitFirstMeetings.Add(set, dialogue);
    }

    [HarmonyPatch(typeof(Neow), "DefineDialogues")]
    internal static class NeowPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "NEOW", BuildShumitDialogues(
                "event:/sfx/npcs/neow/neow_welcome",
                "event:/sfx/npcs/neow/neow_curious",
                "event:/sfx/npcs/neow/neow_sleepy",
                "event:/sfx/npcs/neow/neow_sleepy"));

            InjectFirstMeeting(__result, "NEOW", lineCount: 5, sfx: "event:/sfx/npcs/neow/neow_welcome");
        }
    }

    [HarmonyPatch(typeof(Darv), "DefineDialogues")]
    internal static class DarvPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "DARV", BuildShumitDialogues(
                "event:/sfx/npcs/darv/darv_introduction",
                "event:/sfx/npcs/darv/darv_excited",
                "event:/sfx/npcs/darv/darv_endeared",
                "event:/sfx/npcs/darv/darv_excited"));
        }
    }

    [HarmonyPatch(typeof(Pael), "DefineDialogues")]
    internal static class PaelPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "PAEL", BuildShumitDialogues("", "", "", ""));
        }
    }

    [HarmonyPatch(typeof(Orobas), "DefineDialogues")]
    internal static class OrobasPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "OROBAS", BuildShumitDialogues("", "", "", ""));
        }
    }

    [HarmonyPatch(typeof(Vakuu), "DefineDialogues")]
    internal static class VakuuPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "VAKUU", BuildShumitDialogues("", "", "", ""));
        }
    }

    [HarmonyPatch(typeof(Tezcatara), "DefineDialogues")]
    internal static class TezcataraPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "TEZCATARA", BuildShumitDialogues("", "", "", ""));
        }
    }

    [HarmonyPatch(typeof(Tanx), "DefineDialogues")]
    internal static class TanxPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "TANX", BuildShumitDialogues("", "", "", ""));
        }
    }

    [HarmonyPatch(typeof(Nonupeipe), "DefineDialogues")]
    internal static class NonupeipePatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref AncientDialogueSet __result)
        {
            Inject(__result, "NONUPEIPE", BuildShumitDialogues("", "", "", ""));
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
                BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("TheArchitect.DefineDialogues not found");
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

/// <summary>
/// 슈미트로 게임을 시작해 NEow를 진짜 처음 만나는 시점(<c>totalVisits == 0</c>)에 본가의
/// <see cref="AncientDialogueSet.FirstVisitEverDialogue"/> 대신 슈미트 전용 "최초의 만남" dialogue를 반환한다.
/// 다른 캐릭터/다른 ancient에는 영향 없음.
/// </summary>
[HarmonyPatch(typeof(AncientDialogueSet), nameof(AncientDialogueSet.GetValidDialogues))]
internal static class FrontierFirstMeetingDialoguePatch
{
    [HarmonyPrefix]
    private static bool Prefix(
        AncientDialogueSet __instance,
        ModelId characterId,
        int totalVisits,
        ref IEnumerable<AncientDialogue> __result)
    {
        if (characterId.Entry != FrontierAncientDialoguesPatch.ShumitCharacterKey) { return true; }
        if (totalVisits != 0) { return true; }
        if (!FrontierAncientDialoguesPatch.ShumitFirstMeetings.TryGetValue(__instance, out AncientDialogue? dialogue)
            || dialogue == null)
        {
            return true;
        }

        __result = new[] { dialogue };
        return false;
    }
}
