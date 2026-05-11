using System;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Frontier.Patches;

/// <summary>
/// 슈미트 캐릭터가 전투/이벤트 시점에 말풍선을 띄울 수 있도록 보조하는 헬퍼.
/// 키 패턴: <c>characters.FRONTIER-SHUMIT_CHARACTER.banter.alive.{suffix}</c>
/// 키가 비어 있거나 캐릭터가 사망 상태면 호출을 무시한다.
/// </summary>
internal static class FrontierShumitBanterHelper
{
    /// <summary>슈미트 <c>characters</c> 테이블 키.</summary>
    public const string ShumitCharacterKey = "FRONTIER-SHUMIT_CHARACTER";

    public static void SayCombatStart(Player player) => Say(player, "combatStart");

    public static void SayBossKilled(Player player) => Say(player, "bossKilled");

    public static void SayLowHp(Player player) => Say(player, "lowHp");

    private static void Say(Player? player, string suffix)
    {
        try
        {
            if (player?.Character?.Id.Entry != ShumitCharacterKey)
            {
                return;
            }

            if (player.Creature == null || player.Creature.IsDead)
            {
                return;
            }

            string key = $"{ShumitCharacterKey}.banter.alive.{suffix}";
            if (!LocString.Exists("characters", key))
            {
                return;
            }

            LocString line = new LocString("characters", key);
            TalkCmd.Play(line, player.Creature, player.Character.SpeechBubbleColor, VfxDuration.Short);
        }
        catch (Exception e)
        {
            GD.Print($"[Frontier] Banter '{suffix}' failed: {e.Message}");
        }
    }
}

/// <summary>
/// 전투가 시작된 직후 슈미트 플레이어라면 <c>combatStart</c> 말풍선을 띄운다.
/// <see cref="CombatManager.StartCombatInternal"/>은 비동기 메서드라 Postfix에서 받은 <see cref="Task"/>를
/// 이어붙이는 식으로 합쳐, 본가 시작 처리가 모두 끝난 다음에 멘트가 실행되게 한다.
/// </summary>
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.StartCombatInternal))]
internal static class FrontierShumitCombatStartBanterPatch
{
    [HarmonyPostfix]
    private static void Postfix(CombatManager __instance, ref Task __result)
    {
        Task original = __result;
        __result = ChainBanter(original, __instance);
    }

    private static async Task ChainBanter(Task original, CombatManager mgr)
    {
        await original;

        try
        {
            CombatState? state = mgr.DebugOnlyGetState();
            if (state == null)
            {
                return;
            }

            Player? me = LocalContext.GetMe(state);
            if (me == null)
            {
                return;
            }

            FrontierShumitBanterHelper.SayCombatStart(me);
        }
        catch (Exception e)
        {
            GD.Print($"[Frontier] Combat start banter failed: {e.Message}");
        }
    }
}
