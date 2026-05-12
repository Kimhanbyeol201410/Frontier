using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Cards;

/// <summary>
/// 무수히 많은 기억 — <c>DustyTome</c>(먼지 쌓인 책) 변환으로만 등장하는 슈미트 전용 Ancient 파워 카드.
/// 사용 시점에 덱·손·뽑을·버릴 더미 카드의 누적 강화 횟수만큼 X 턴 동안
/// <see cref="CountlessMemoriesPower"/> 를 부여하여 매 턴 시작 시 에너지 2 + 드로우 4 를 얻는다.
///
/// <para>강화는 1 회만 가능 — 슈미트 «재련» 시스템에서 제외(<c>FrontierRules.GetReforgeBonus</c> 참고).
/// 강화 시 <see cref="CardKeyword.Innate"/> 가 추가된다.</para>
/// </summary>
[Pool(typeof(ShumitCardPool))]
public sealed class CountlessMemoriesCard : ShumitCard
{
    public CountlessMemoriesCard()
        : base(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar("EnergyPerTurn", CountlessMemoriesPower.EnergyPerTurn),
        new CardsVar("DrawPerTurn", CountlessMemoriesPower.DrawPerTurn),
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int totalUpgrades = SumUpgradeLevels(Owner);
        if (totalUpgrades <= 0)
        {
            return;
        }

        await PowerCmd.Apply<CountlessMemoriesPower>(
            Owner.Creature,
            totalUpgrades,
            Owner.Creature,
            this,
            silent: false);
    }

    private static int SumUpgradeLevels(MegaCrit.Sts2.Core.Entities.Players.Player owner)
    {
        IEnumerable<MegaCrit.Sts2.Core.Models.CardModel> all = owner.Deck.Cards
            .Concat(PileType.Hand.GetPile(owner).Cards)
            .Concat(PileType.Draw.GetPile(owner).Cards)
            .Concat(PileType.Discard.GetPile(owner).Cards);

        return all.Sum(c => c.CurrentUpgradeLevel);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
