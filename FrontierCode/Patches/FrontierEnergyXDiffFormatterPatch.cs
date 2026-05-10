using Frontier.Cards;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization.Formatters;
using SmartFormat.Core.Extensions;

namespace Frontier.Patches;

/// <summary><see cref="EnergyXVar"/>는 <c>{X:diff()}</c>에서 글자 X / 소모 에너지 숫자를 쓰도록 한다. 코어 <see cref="DynamicVar.ToHighlightedString"/>이 virtual이 아니어서 포매터를 가로챈다.</summary>
[HarmonyPatch(typeof(HighlightDifferencesFormatter), nameof(HighlightDifferencesFormatter.TryEvaluateFormat))]
public static class FrontierEnergyXDiffFormatterPatch
{
    public static bool Prefix(IFormattingInfo formattingInfo, ref bool __result)
    {
        if (formattingInfo.CurrentValue is EnergyXVar ex)
        {
            formattingInfo.Write(ex.FormatForDiffHighlight(inverse: false));
            __result = true;
            return false;
        }

        return true;
    }
}
