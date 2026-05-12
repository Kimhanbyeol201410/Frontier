using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Frontier.Cards;
using Frontier.Characters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;

namespace Frontier.Relics;

/// <summary>
/// 「신의 눈」 — 「판단의 눈」(<see cref="BrokenForgeRelic"/>) 의 강화 시작 유물.
/// «오로바스의 손길»(<c>TouchOfOrobas</c>) 변환으로만 획득 — 일반 보상 풀에는 등장하지 않는다.
///
/// <para>효과:</para>
/// <list type="bullet">
///   <item><description>모든 슈미트 카드의 «재련» 최소 보장 <b>2 → 4</b>. (<c>FrontierRules.GetReforgeBonus</c>)</description></item>
///   <item><description>모든 슈미트 «걸작» 카드의 변환 기준 <b>-5</b>. (<c>FrontierRules.GetMasterpieceValue</c>)</description></item>
/// </list>
///
/// <para>획득 시점에 이미 덱·전투 더미·손패·버린 카드 더미에 존재하는 슈미트 걸작 카드 인스턴스의
/// <c>MasterpieceLeft</c> 표시값을 -5 동기화하여, 카드 텍스트와 실제 변환 시점이 일치하도록 한다.</para>
/// </summary>
[Pool(typeof(EventRelicPool))]
public sealed class DivineEyeRelic : CustomRelicModel
{
    /// <summary>걸작 카드 표시 카운터에서 차감하는 값. <see cref="FrontierRules.GetMasterpieceValue"/> 와 동일 값 유지.</summary>
    public const int MasterpieceReduction = 5;

    private const string MasterpieceLeftKey = "MasterpieceLeft";

    public override RelicRarity Rarity => RelicRarity.Starter;

    /// <summary>일반 보상 풀에서 등장하지 않음 — «오로바스의 손길» 변환 전용.</summary>
    public override bool IsAllowed(IRunState runState) => false;

    public override async Task AfterObtained()
    {
        if (base.Owner == null)
        {
            return;
        }

        foreach (CardModel card in EnumerateAllCards(base.Owner))
        {
            ApplyMasterpieceReduction(card);
        }

        await Task.CompletedTask;
    }

    /// <summary>새로 생성·획득되는 슈미트 걸작 카드에도 차감을 적용. <see cref="ShumitCard.AfterCreated"/> 가 본 메서드를 호출한다.</summary>
    public static void ApplyMasterpieceReductionIfApplicable(CardModel card)
    {
        if (card.Owner?.GetRelic<DivineEyeRelic>() == null)
        {
            return;
        }

        ApplyMasterpieceReduction(card);
    }

    private static void ApplyMasterpieceReduction(CardModel card)
    {
        if (card.Id?.Entry == null) return;
        if (FrontierRules.GetMasterpieceValueRaw(card) <= 0) return;
        if (!card.DynamicVars.ContainsKey(MasterpieceLeftKey)) return;

        DynamicVar var = card.DynamicVars[MasterpieceLeftKey];
        var.UpgradeValueBy(-(decimal)MasterpieceReduction);
    }

    private static IEnumerable<CardModel> EnumerateAllCards(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        // 비전투 시(휴식처·이벤트) Deck 만 유효. 전투 중에는 손/뽑을/버린/소진 더미까지 동기화.
        IEnumerable<CardModel> cards = player.Deck.Cards;

        if (CombatManager.Instance != null && !CombatManager.Instance.IsOverOrEnding)
        {
            cards = cards
                .Concat(PileType.Hand.GetPile(player).Cards)
                .Concat(PileType.Draw.GetPile(player).Cards)
                .Concat(PileType.Discard.GetPile(player).Cards)
                .Concat(PileType.Exhaust.GetPile(player).Cards);
        }

        return cards.Where(c => c.Owner?.Character?.Id?.Entry == ShumitCharacter.CharacterId);
    }
}
