using System;
using Frontier.Cards;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Relics;

public abstract class TokenSpawnerRelic<TCard> : CustomRelicModel where TCard : CardModel
{
    protected TokenSpawnerRelic() { }

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.CombatState is not CombatState combatState)
        {
            throw new InvalidOperationException("TokenSpawnerRelic.BeforeCombatStart requires an active CombatState.");
        }

        CardModel token = combatState.CreateCard<TCard>(Owner);
        await CardPileCmd.AddGeneratedCardsToCombat(new[] { token }, PileType.Draw, Owner, CardPilePosition.Random);
        Flash();
    }
}

[Pool(typeof(EventRelicPool))]
public sealed class GreatForgeRelic : TokenSpawnerRelic<GreatForgeCard>
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public GreatForgeRelic() { }
}

[Pool(typeof(EventRelicPool))]
public sealed class HeatproofApronRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 추후 구현
}

[Pool(typeof(EventRelicPool))]
public sealed class SmelterShardRelic : TokenSpawnerRelic<SmelterCard>
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public SmelterShardRelic() { }
}

[Pool(typeof(EventRelicPool))]
public sealed class HeartOfFlameRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 異뷀썑 援ы쁽
}

[Pool(typeof(EventRelicPool))]
public sealed class BlastFurnaceShardRelic : TokenSpawnerRelic<BlastFurnaceCard>
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public BlastFurnaceShardRelic() { }
}

[Pool(typeof(EventRelicPool))]
public sealed class GrindingRoomShardRelic : TokenSpawnerRelic<GrindingRoomCard>
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public GrindingRoomShardRelic() { }
}

[Pool(typeof(EventRelicPool))]
public sealed class HephaestusBloodRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 추후 구현
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
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 異뷀썑 援ы쁽
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
        await PowerCmd.Apply<StrengthPower>(context, new[] { base.Owner.Creature }, 1m, base.Owner.Creature, cardPlay.Card, false);
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

