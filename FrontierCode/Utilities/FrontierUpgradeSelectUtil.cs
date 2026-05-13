using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Frontier.Utilities;

/// <summary>
/// 손패에서 0 ~ N 장을 강화 선택할 수 있는 유틸.
///
/// <para>모드 선택 정책:
/// <list type="bullet">
///   <item><description><paramref name="maxCount"/> 가 1 인 경우 <see cref="NPlayerHand.Mode.UpgradeSelect"/> — 강화 후 미리보기 카드가 우측에 표시된다.</description></item>
///   <item><description><paramref name="maxCount"/> 가 2 이상인 경우 <see cref="NPlayerHand.Mode.SimpleSelect"/> — UpgradeSelect 모드는 한 번에 한 장만 유지하도록 구현되어 있어 다중 선택이 불가능하다.</description></item>
/// </list></para>
///
/// <para>※ 멀티플레이/리플레이 동기화는 본 유틸의 책임 범위 밖이며, 슈미트 모드는 로컬 단독 플레이를 기준으로 한다.
/// 손패에 강화 대상이 없으면 선택 화면을 띄우지 않고 빈 결과를 반환해 softlock 을 방지한다.</para>
/// </summary>
public static class FrontierUpgradeSelectUtil
{
    /// <summary>강화 선택 모드로 0 ~ <paramref name="maxCount"/> 장 선택. 빈 결과 = 스킵.</summary>
    public static async Task<IReadOnlyList<CardModel>> SelectFromHandWithPreviewAsync(
        Player owner,
        AbstractModel source,
        int maxCount,
        Func<CardModel, bool>? extraFilter = null)
    {
        if (maxCount <= 0)
        {
            return Array.Empty<CardModel>();
        }

        if (CombatManager.Instance.IsOverOrEnding)
        {
            return Array.Empty<CardModel>();
        }

        if (NCombatRoom.Instance?.Ui?.Hand is not { } hand)
        {
            return Array.Empty<CardModel>();
        }

        bool Filter(CardModel c)
            => c.IsUpgradable && !ReferenceEquals(c, source) && (extraFilter?.Invoke(c) ?? true);

        // 강화 가능한 카드가 없으면 선택 화면을 띄우지 않는다 (softlock 방지).
        List<CardModel> upgradable = PileType.Hand.GetPile(owner).Cards.Where(Filter).ToList();
        if (upgradable.Count == 0)
        {
            return Array.Empty<CardModel>();
        }

        // UpgradeSelect 는 _selectedCards 갱신 시 이전 선택을 자동 해제하기 때문에
        // 2 장 이상 선택을 받으려면 SimpleSelect 로 폴백해야 한다.
        NPlayerHand.Mode mode = maxCount == 1 ? NPlayerHand.Mode.UpgradeSelect : NPlayerHand.Mode.SimpleSelect;

        CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, 0, maxCount);
        IEnumerable<CardModel> picked = await hand.SelectCards(prefs, Filter, source, mode);
        return picked.ToList();
    }
}
