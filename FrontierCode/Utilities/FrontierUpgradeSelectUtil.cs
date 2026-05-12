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
/// 손패에서 0 ~ N 장을 «강화 미리보기 모드»(<see cref="NPlayerHand.Mode.UpgradeSelect"/>) 로 선택할 수 있는 유틸.
///
/// <para>게임 본체의 <c>CardSelectCmd.FromHandForUpgrade</c> 는 미리보기를 띄우지만 1 장 강제 선택만 가능하고,
/// <c>CardSelectCmd.FromHand</c> 는 0 ~ N 자유 선택이 가능한 대신 미리보기 모드 표시를 잃는다.
/// 두 동작을 결합하기 위해 <see cref="NPlayerHand.SelectCards"/> 를 모드 인자와 함께 직접 호출한다.</para>
///
/// <para>※ 멀티플레이/리플레이 동기화는 본 유틸의 책임 범위 밖이며, 슈미트 모드는 로컬 단독 플레이를 기준으로 한다.
/// 손패에 강화 대상이 없으면 선택 화면을 띄우지 않고 빈 결과를 반환해 softlock 을 방지한다.</para>
/// </summary>
public static class FrontierUpgradeSelectUtil
{
    /// <summary>강화 미리보기 모드로 0 ~ <paramref name="maxCount"/> 장 선택. 빈 결과 = 스킵.</summary>
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

        CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, 0, maxCount);
        IEnumerable<CardModel> picked = await hand.SelectCards(prefs, Filter, source, NPlayerHand.Mode.UpgradeSelect);
        return picked.ToList();
    }
}
