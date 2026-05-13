using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Frontier;
using Frontier.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Relics;

[Pool(typeof(ShumitRelicPool))]
public sealed class HeatproofApronRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount <= 0m || base.Owner?.Creature == null || !base.Owner.Creature.IsPlayer)
        {
            return;
        }

        if (power.Owner != base.Owner.Creature || power.Id.Entry != "FRONTIER-HEAT_POWER")
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(base.Owner.Creature, 2m, ValueProp.Unpowered, null, fast: true);
    }
}

[Pool(typeof(ShumitRelicPool))]
public sealed class HephaestusBloodRelic : CustomRelicModel
{
    private const int HeatPerStrength = 20;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (base.Owner?.Creature == null || !base.Owner.Creature.IsPlayer)
        {
            return;
        }

        if (power.Owner != base.Owner.Creature || power.Id.Entry != "FRONTIER-HEAT_POWER")
        {
            return;
        }

        int newAmount = power.Amount;
        int oldAmount = newAmount - (int)amount;
        int diff = (newAmount / HeatPerStrength) - (oldAmount / HeatPerStrength);
        if (diff > 0)
        {
            Flash();
            await PowerCmd.Apply<StrengthPower>(new[] { base.Owner.Creature }, diff, base.Owner.Creature, null, false);
        }
    }
}

[Pool(typeof(EventRelicPool))]
public sealed class AncientAnvilRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner || cardPlay.Card.CurrentUpgradeLevel <= 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(base.Owner.Creature, 3m, ValueProp.Unpowered, cardPlay, fast: true);
    }
}

[Pool(typeof(EventRelicPool))]
public sealed class FusionerHammerRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    // 카드 사용 「전」에 [활력] 을 부여한다. AfterCardPlayed 로 두면 카드 자신의 공격(특히 멀티히트)에는
    // 적용되지 않으므로, BeforeCardPlayed 로 옮겨 사용 카드의 첫 공격부터 활력 보너스가 들어가게 한다.
    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner || cardPlay.Card.CurrentUpgradeLevel <= 0)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<VigorPower>(new[] { base.Owner.Creature }, 5m, base.Owner.Creature, cardPlay.Card, false);
    }
}

[Pool(typeof(EventRelicPool))]
public sealed class FusionerTongsRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner || cardPlay.Card.CurrentUpgradeLevel <= 0)
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(context, 1, base.Owner);
    }
}

[Pool(typeof(EventRelicPool))]
public sealed class FusionerAnvilRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner || cardPlay.Card.CurrentUpgradeLevel <= 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(base.Owner.Creature, 5m, ValueProp.Unpowered, cardPlay, fast: true);
    }
}

internal static class FrontierShumitRelicGate
{
    internal static bool IsShumitRun(IRunState runState)
    {
        return runState.Players.Any(p => p.Character?.Id?.Entry == ShumitCharacter.CharacterId);
    }
}

/// <summary>끝없는 노력 — 휴식처 모루 행동 시 강화 카드 2장 추가 선택 가능.</summary>
[Pool(typeof(ShumitRelicPool))]
public sealed class EndlessLaborRelic : CustomRelicModel
{
    private const int ExtraSmithCount = 2;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool IsAllowed(IRunState runState) => FrontierShumitRelicGate.IsShumitRun(runState);

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != base.Owner)
        {
            return false;
        }

        bool modified = false;
        foreach (SmithRestSiteOption smith in options.OfType<SmithRestSiteOption>())
        {
            smith.SmithCount += ExtraSmithCount;
            modified = true;
        }

        return modified;
    }
}

/// <summary>타지않는 육체 — [신체 화상] 임계값을 200 → 300으로 변경. 실제 임계값 변경은 <c>HeatPower.AfterTurnEnd</c>에서 처리.</summary>
[Pool(typeof(ShumitRelicPool))]
public sealed class UnburnableBodyRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool IsAllowed(IRunState runState) => FrontierShumitRelicGate.IsShumitRun(runState);
}

/// <summary>걸작 박물관 — 전투 시작 시 보유한 걸작 카드 1장당 힘 2, 민첩 2, 에너지 2를 얻고 시작.</summary>
[Pool(typeof(ShumitRelicPool))]
public sealed class MasterpieceMuseumRelic : CustomRelicModel
{
    private const int BonusPerMasterpiece = 2;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool IsAllowed(IRunState runState) => FrontierShumitRelicGate.IsShumitRun(runState);

    public override async Task BeforeCombatStart()
    {
        if (base.Owner == null || base.Owner.Creature == null)
        {
            return;
        }

        int count = 0;
        foreach (CardModel card in base.Owner.Deck.Cards)
        {
            if (FrontierRules.GetMasterpieceValue(card) > 0)
            {
                count++;
            }
        }

        if (count <= 0)
        {
            return;
        }

        Flash();
        decimal stat = BonusPerMasterpiece * count;
        await PowerCmd.Apply<StrengthPower>(new[] { base.Owner.Creature }, stat, base.Owner.Creature, null, false);
        await PowerCmd.Apply<DexterityPower>(new[] { base.Owner.Creature }, stat, base.Owner.Creature, null, false);
        await PlayerCmd.GainEnergy((int)stat, base.Owner);
    }
}

/// <summary>무한히 불타는 화로 — 전투 시작 시 [열기] 70 즉시 획득, 매 [화상] 드로우 시 화상 카드를 소진시키고 카드 1장 드로우 + [열기] 20 획득.</summary>
[Pool(typeof(ShumitRelicPool))]
public sealed class EternallyBurningFurnaceRelic : CustomRelicModel
{
    private const int InitialHeat = 70;
    private const int HeatPerBurnDrawn = 20;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool IsAllowed(IRunState runState) => FrontierShumitRelicGate.IsShumitRun(runState);

    public override async Task BeforeCombatStart()
    {
        if (base.Owner?.Creature == null)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<HeatPower>(base.Owner.Creature, InitialHeat, base.Owner.Creature, null);
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card is not Burn || base.Owner == null || base.Owner.Creature == null)
        {
            return;
        }

        if (card.Owner != base.Owner)
        {
            return;
        }

        if (oldPileType != PileType.Draw || card.Pile?.Type != PileType.Hand)
        {
            return;
        }

        Flash();
        await CardPileCmd.Add(card, PileType.Exhaust, CardPilePosition.Bottom, this);
        await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), 1, base.Owner);
        await PowerCmd.Apply<HeatPower>(base.Owner.Creature, HeatPerBurnDrawn, base.Owner.Creature, null);
    }
}
