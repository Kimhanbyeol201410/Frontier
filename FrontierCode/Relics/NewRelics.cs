using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Frontier.Cards.Token;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Frontier.Relics;

public abstract class TokenSpawnerRelic<TCard> : CustomRelicModel where TCard : CardModel
{
    private readonly string _title;
    private readonly string _desc;
    protected TokenSpawnerRelic(string title, string desc) { _title = title; _desc = desc; }
    public override List<(string, string)> Localization => new() { ("title", _title), ("description", _desc) };
    public override async Task BeforeCombatStart()
    {
        CardModel token = Owner.RunState.CreateCard<TCard>(Owner);
        await CardPileCmd.Add(token, PileType.Draw, CardPilePosition.Random);
        Flash();
    }
}

[Pool(typeof(EventRelicPool))]
public sealed class GreatForgeRelic : TokenSpawnerRelic<GreatForgeCard>
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public GreatForgeRelic() : base("위대한 대장간", "전투 시작 시 뽑을 카드 더미에 [gold]위대한 대장간[/gold] 1장을 추가합니다.") { }
}

[Pool(typeof(EventRelicPool))]
public sealed class HeatproofApronRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;
    public override List<(string, string)> Localization => new() { ("title", "내열 가죽 앞치마"), ("description", "열기를 얻을 때마다 방어도 2를 얻습니다.") };
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 추후 구현
}

[Pool(typeof(EventRelicPool))]
public sealed class SmelterShardRelic : TokenSpawnerRelic<SmelterCard>
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public SmelterShardRelic() : base("제련소의 파편", "전투 시작 시 뽑을 카드 더미에 [gold]제련소[/gold] 1장을 추가합니다.") { }
}

[Pool(typeof(EventRelicPool))]
public sealed class HeartOfFlameRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override List<(string, string)> Localization => new() { ("title", "화염의 심장"), ("description", "화상과 신체 화상 피해를 받지 않습니다.") };
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 추후 구현
}

[Pool(typeof(EventRelicPool))]
public sealed class BlastFurnaceShardRelic : TokenSpawnerRelic<BlastFurnaceCard>
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public BlastFurnaceShardRelic() : base("용광로의 파편", "전투 시작 시 뽑을 카드 더미에 [gold]용광로[/gold] 1장을 추가합니다.") { }
}

[Pool(typeof(EventRelicPool))]
public sealed class GrindingRoomShardRelic : TokenSpawnerRelic<GrindingRoomCard>
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public GrindingRoomShardRelic() : base("연마실의 파편", "전투 시작 시 뽑을 카드 더미에 [gold]연마실[/gold] 1장을 추가합니다.") { }
}

[Pool(typeof(EventRelicPool))]
public sealed class HephaestusBloodRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override List<(string, string)> Localization => new() { ("title", "헤파이스토스의 피"), ("description", "열기 20마다 힘을 1 얻습니다.") };
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 추후 구현
}

[Pool(typeof(EventRelicPool))]
public sealed class AncientAnvilRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Shop;
    public override List<(string, string)> Localization => new() { ("title", "오래된 모루"), ("description", "강화할 때마다 방어도 3을 얻습니다.") };
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 추후 구현
}

[Pool(typeof(EventRelicPool))]
public sealed class FusionerHammerRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override List<(string, string)> Localization => new() { ("title", "융합자의 망치"), ("description", "획득 시 최대 3장의 공격 카드에 예리 5를 인챈트합니다.") };
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 추후 구현
}

[Pool(typeof(EventRelicPool))]
public sealed class FusionerTongsRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override List<(string, string)> Localization => new() { ("title", "융합자의 집게"), ("description", "강화할 때마다 활력 5를 얻습니다.") };
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 추후 구현
}

[Pool(typeof(EventRelicPool))]
public sealed class FusionerAnvilRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override List<(string, string)> Localization => new() { ("title", "융합자의 모루"), ("description", "강화할 때마다 방어도 5를 얻습니다.") };
    // TODO: Heat/Reforge/Masterpiece/Enchant/CondPlay 추후 구현
}
