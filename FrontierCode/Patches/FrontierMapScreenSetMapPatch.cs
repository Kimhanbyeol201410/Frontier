using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace Frontier.Patches;

/// <summary>
/// <see cref="RunManager.GenerateMap"/> 가 <see cref="NRun"/> 씬의 <c>_Ready</c>보다 먼저
/// <see cref="NMapScreen.SetMap"/> 을 호출하면 <c>_points</c>/<c>_marker</c> 가 아직 없어 NRE가 난다.
/// 노드가 준비될 때까지 <see cref="NMapScreen.SetMap"/> 호출을 다음 프레임으로 미룬다.
/// </summary>
[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
internal static class FrontierMapScreenSetMapPatch
{
    [HarmonyPrefix]
    private static bool Prefix(NMapScreen __instance, ActMap map, uint seed, bool clearDrawings)
    {
        if (map == null)
        {
            return false;
        }

        if (__instance.IsNodeReady())
        {
            return true;
        }

        Callable.From(() => __instance.SetMap(map, seed, clearDrawings)).CallDeferred();
        return false;
    }
}
