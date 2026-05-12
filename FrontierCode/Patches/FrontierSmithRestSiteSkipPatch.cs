using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Frontier.Characters;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Frontier.Patches;

/// <summary>
/// 슈미트 한정 — 휴식 사이트의 «대장간»(<see cref="SmithRestSiteOption"/>) 강화에서
/// <c>SmithCount</c> 만큼 «모두» 선택해야 진행되던 것을 0 ~ <c>SmithCount</c> 자유 선택으로 변경.
///
/// <para>다른 캐릭터는 원본 동작(고정 N 장 강제 선택)을 유지하기 위해 <c>Owner.Character.Id.Entry</c> 가
/// 슈미트일 때만 <c>Prefix</c> 가 원본을 대체한다.</para>
///
/// <para>원본 메서드의 <c>private IEnumerable&lt;CardModel&gt;? _selection</c> 필드를 후속 VFX(<c>DoLocalPostSelectVfx</c>·
/// <c>DoRemotePostSelectVfx</c>) 가 참조하므로 동일한 값을 채워 호환성을 유지한다.</para>
/// </summary>
[HarmonyPatch(typeof(SmithRestSiteOption), nameof(SmithRestSiteOption.OnSelect))]
internal static class FrontierSmithRestSiteSkipPatch
{
    private static readonly FieldInfo SelectionField =
        AccessTools.Field(typeof(SmithRestSiteOption), "_selection")
        ?? throw new System.InvalidOperationException("SmithRestSiteOption._selection field not found.");

    private static readonly PropertyInfo OwnerProperty =
        AccessTools.Property(typeof(RestSiteOption), "Owner")
        ?? throw new System.InvalidOperationException("RestSiteOption.Owner property not found.");

    [HarmonyPrefix]
    private static bool Prefix(SmithRestSiteOption __instance, ref Task<bool> __result)
    {
        Player? owner = OwnerProperty.GetValue(__instance) as Player;
        if (owner?.Character?.Id?.Entry != ShumitCharacter.CharacterId)
        {
            return true;
        }

        __result = RunWithSkip(__instance, owner);
        return false;
    }

    private static async Task<bool> RunWithSkip(SmithRestSiteOption option, Player owner)
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 0, option.SmithCount)
        {
            Cancelable = true,
            RequireManualConfirmation = true,
        };

        IEnumerable<CardModel> selection = await CardSelectCmd.FromDeckForUpgrade(owner, prefs);
        List<CardModel> picked = selection.ToList();
        // VFX 단계가 _selection 필드를 참조하므로 비어 있어도 동일한 컬렉션을 보존한다.
        SelectionField.SetValue(option, picked);

        if (picked.Count == 0)
        {
            return false;
        }

        foreach (CardModel item in picked)
        {
            CardCmd.Upgrade(item, CardPreviewStyle.None);
        }

        await Hook.AfterRestSiteSmith(owner.RunState, owner);
        return true;
    }
}
