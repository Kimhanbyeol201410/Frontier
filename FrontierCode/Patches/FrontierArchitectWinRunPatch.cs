using System;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;

namespace Frontier.Patches;

/// <summary>
/// 모드 캐릭터(슈미트)는 <see cref="TheArchitect"/>의 <c>CharacterDialogues</c>에 등록되어 있지 않다.
/// 그래서 <c>LoadDialogue</c>가 빈 결과를 돌려 <c>_dialogue</c>가 <see langword="null"/>이 되고,
/// 플레이어가 Proceed 옵션을 누르면 <c>WinRun</c>이 <c>Dialogue.EndAttackers</c>에 접근해
/// <see cref="NullReferenceException"/>을 던지며 런 결산이 막힌다.
/// 이 패치는 Dialogue가 null인 경우에만 원본을 건너뛰고
/// <c>ActChangeSynchronizer.SetLocalPlayerReady()</c>를 직접 호출해 정상적으로 런을 마무리한다.
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "WinRun")]
internal static class FrontierArchitectWinRunPatch
{
    private static readonly FieldInfo? _dialogueField =
        typeof(TheArchitect).GetField("_dialogue", BindingFlags.NonPublic | BindingFlags.Instance);

    [HarmonyPrefix]
    private static bool Prefix(TheArchitect __instance, ref Task __result)
    {
        object? dialogue = _dialogueField?.GetValue(__instance);
        if (dialogue != null)
        {
            return true;
        }

        __result = SafeFinishRun(__instance);
        return false;
    }

    private static async Task SafeFinishRun(TheArchitect instance)
    {
        try
        {
            if (instance.Owner != null && LocalContext.IsMe(instance.Owner))
            {
                RunManager.Instance.ActChangeSynchronizer.SetLocalPlayerReady();
                GD.Print("[Frontier] TheArchitect.WinRun: dialogue was null, finishing run directly.");
            }
        }
        catch (Exception e)
        {
            GD.Print($"[Frontier] TheArchitect.WinRun bypass failed: {e.Message}");
        }
        await Task.CompletedTask;
    }
}
