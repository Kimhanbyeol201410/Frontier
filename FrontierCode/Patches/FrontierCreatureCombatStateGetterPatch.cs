using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Frontier.Utilities;

namespace Frontier.Patches;

/// <summary>
/// 코어가 <c>Creature.CombatState</c>를 읽을 때, 전부 <see cref="CombatManager"/>만 보면
/// <see cref="CombatState.AttachCreature"/> 직후·신규 전투 구성 중 필드와 불일치해
/// "Creature was created for a different combat" 가 난다.
/// 자동 프로퍼티 백킹 필드를 먼저 사용하고, 비어 있을 때만 헬퍼로 폴백한다.
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.CombatState), MethodType.Getter)]
internal static class FrontierCreatureCombatStateGetterPatch
{
    private static readonly FieldInfo? CombatStateBackingField = ResolveCombatStateBackingField();

    private static FieldInfo? ResolveCombatStateBackingField()
    {
        FieldInfo? f = AccessTools.DeclaredField(typeof(Creature), "<CombatState>k__BackingField");
        if (f != null)
        {
            return f;
        }

        foreach (FieldInfo candidate in typeof(Creature).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (candidate.Name.Contains("CombatState", StringComparison.Ordinal)
                && candidate.Name.Contains("BackingField", StringComparison.Ordinal))
            {
                return candidate;
            }
        }

        return null;
    }

    [HarmonyPrefix]
    private static bool Prefix(Creature __instance, ref CombatState? __result)
    {
        if (CombatStateBackingField != null)
        {
            __result = (CombatState?)CombatStateBackingField.GetValue(__instance);
            if (__result != null)
            {
                return false;
            }
        }

        __result = FrontierCombatStateHelper.TryGetForCreature(__instance);
        return false;
    }
}
