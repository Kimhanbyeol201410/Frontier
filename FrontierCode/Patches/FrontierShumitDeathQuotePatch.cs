using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace Frontier.Patches;

/// <summary>
/// 게임 오버 화면(<see cref="NGameOverScreen"/>)의 첫 패배 인용구는 본가 <c>game_over_screen.QUOTES.*</c>
/// 풀에서 무작위로 뽑힌다. 본가 풀과 분리된 슈미트 전용 풀
/// <c>game_over_screen.FRONTIER-SHUMIT_CHARACTER.QUOTES.*</c> 키가 존재하면, 슈미트 플레이어가 패배했을 때
/// 그 풀에서 다시 뽑아 화면 라벨(<c>_deathQuote</c>)을 덮어쓴다.
///
/// 본가 <c>QUOTES</c> prefix와 충돌하지 않도록 슈미트 전용 풀은 다른 prefix를 사용한다.
/// 승리 시(<see cref="RunHistory.Win"/> = <see langword="true"/>)에는 본가가 라벨을 비워두므로 건드리지 않는다.
/// </summary>
[HarmonyPatch(typeof(NGameOverScreen), "InitializeBannerAndQuote")]
internal static class FrontierShumitDeathQuotePatch
{
    private const string ShumitCharacterKey = "FRONTIER-SHUMIT_CHARACTER";
    private const string QuotePrefix = "FRONTIER-SHUMIT_CHARACTER.QUOTES";

    private static readonly FieldInfo? _localPlayerField =
        typeof(NGameOverScreen).GetField("_localPlayer", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo? _historyField =
        typeof(NGameOverScreen).GetField("_history", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo? _deathQuoteField =
        typeof(NGameOverScreen).GetField("_deathQuote", BindingFlags.NonPublic | BindingFlags.Instance);

    [HarmonyPostfix]
    private static void Postfix(NGameOverScreen __instance)
    {
        try
        {
            if (_localPlayerField?.GetValue(__instance) is not Player localPlayer)
            {
                return;
            }

            if (_historyField?.GetValue(__instance) is not RunHistory history || history.Win)
            {
                return;
            }

            if (localPlayer.Character?.Id.Entry != ShumitCharacterKey)
            {
                return;
            }

            IReadOnlyList<LocString> quotes = LocManager.Instance
                .GetTable("game_over_screen")
                .GetLocStringsWithPrefix(QuotePrefix);
            if (quotes.Count == 0)
            {
                return;
            }

            if (_deathQuoteField?.GetValue(__instance) is not MegaRichTextLabel deathLabel)
            {
                return;
            }

            LocString? picked = Rng.Chaotic.NextItem(quotes);
            if (picked == null)
            {
                return;
            }
            deathLabel.Text = picked.GetFormattedText();
        }
        catch (Exception e)
        {
            GD.Print($"[Frontier] DeathQuote override failed: {e.Message}");
        }
    }
}
