using Frontier.Cards;
using Godot.Bridge;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using System.Collections.Generic;
using System.Linq;
using Frontier.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using BaseLib.Patches.UI;
using Frontier.Localization;
using Frontier.Relics;
using System.Threading.Tasks;

[ModInitializer("ModInit")]
public static class ModStart
{
    public static void ModInit()
    {
        // BaseLib/Pool 모델 인식을 위해 어셈블리 스크립트 등록
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(ModStart).Assembly);

		Harmony harmony = new Harmony("sts2-frontier");
        harmony.PatchAll();

        RegisterRelicPlaceholderIcons();

        GD.Print("[Frontier] Mod Initialized");
    }

    private static void RegisterRelicPlaceholderIcons()
    {
        var icon = new RelicIconData(FrontierAssetPaths.VanillaRelicFallbackPng, FrontierAssetPaths.VanillaRelicFallbackPng, FrontierAssetPaths.VanillaRelicFallbackPng);
        RelicImageOverridePatch.AddOverride<BrokenForgeRelic>(icon);
        RelicImageOverridePatch.AddOverride<GreatForgeRelic>(icon);
        RelicImageOverridePatch.AddOverride<HeatproofApronRelic>(icon);
        RelicImageOverridePatch.AddOverride<SmelterShardRelic>(icon);
        RelicImageOverridePatch.AddOverride<HeartOfFlameRelic>(icon);
        RelicImageOverridePatch.AddOverride<BlastFurnaceShardRelic>(icon);
        RelicImageOverridePatch.AddOverride<GrindingRoomShardRelic>(icon);
        RelicImageOverridePatch.AddOverride<HephaestusBloodRelic>(icon);
        RelicImageOverridePatch.AddOverride<AncientAnvilRelic>(icon);
        RelicImageOverridePatch.AddOverride<FusionerHammerRelic>(icon);
        RelicImageOverridePatch.AddOverride<FusionerTongsRelic>(icon);
        RelicImageOverridePatch.AddOverride<FusionerAnvilRelic>(icon);
    }
}

internal static class FrontierRules
{
    // 재련/걸작 수치가 0 이하이면 "무한 강화"로 취급합니다.
    public const int InfiniteUpgradeCap = 999;
    private static readonly Dictionary<string, int> ReforgeByCardId = new()
    {
        ["BellowsCard"] = 1,
        ["AnvilMemoryCard"] = 1,
        ["BurningStrikeCard"] = 1,
        ["SparkBurstCard"] = 10,
        ["SteamReleaseCard"] = 10,
    };

    private static readonly Dictionary<string, int> MasterpieceByCardId = new()
    {
        ["AnvilEchoCard"] = 10
    };
    private static readonly Dictionary<string, System.Func<MegaCrit.Sts2.Core.Entities.Players.Player, CardModel>> MasterpieceTransformByCardId = new()
    {
        ["AnvilEchoCard"] = static (MegaCrit.Sts2.Core.Entities.Players.Player owner) =>
            owner.Creature.CombatState!.CreateCard<AnvilMemoryCard>(owner)
    };

    public static bool IsShumit(CardModel card)
    {
        return card.Owner?.Character?.Id?.Entry == ShumitCharacter.CharacterId;
    }

    public static int GetReforgeBonus(CardModel card)
    {
        if (card?.Id?.Entry == null)
        {
            return 0;
        }

        if (ReforgeByCardId.TryGetValue(card.Id.Entry, out int bonus))
        {
            return bonus;
        }

        return 0;
    }

    public static int GetMasterpieceValue(CardModel card)
    {
        if (card?.Id?.Entry == null)
        {
            return 0;
        }

        if (MasterpieceByCardId.TryGetValue(card.Id.Entry, out int value))
        {
            return value;
        }

        return 0;
    }

    public static System.Func<MegaCrit.Sts2.Core.Entities.Players.Player, CardModel> GetMasterpieceTransformFactory(CardModel card)
    {
        if (card?.Id?.Entry == null)
        {
            return null;
        }

        if (MasterpieceTransformByCardId.TryGetValue(card.Id.Entry, out System.Func<MegaCrit.Sts2.Core.Entities.Players.Player, CardModel> factory))
        {
            return factory;
        }

        return null;
    }
}

[HarmonyPatch(typeof(CardModel), "get_MaxUpgradeLevel")]
public static class FrontierUpgradeCapPatch
{
    public static void Postfix(CardModel __instance, ref int __result)
    {
        if (!FrontierRules.IsShumit(__instance) || __result <= 0)
        {
            return;
        }

        int reforge = FrontierRules.GetReforgeBonus(__instance);
        int masterpiece = FrontierRules.GetMasterpieceValue(__instance);
        // 슈미트.md 기준: "재련" 키워드가 없는 카드는 최대 강화 1.
        if (reforge <= 0)
        {
            __result = 1;
            return;
        }

        __result += reforge + masterpiece;
    }
}

[HarmonyPatch(typeof(CardCmd), "Upgrade", new[] { typeof(IEnumerable<CardModel>), typeof(CardPreviewStyle) })]
public static class FrontierMasterpieceTransformPatch
{
    private static readonly HashSet<int> TriggeredCards = new();

    public static void Postfix(IEnumerable<CardModel> cards)
    {
        foreach (CardModel card in cards.ToList())
        {
            if (!FrontierRules.IsShumit(card))
            {
                continue;
            }

            int masterpiece = FrontierRules.GetMasterpieceValue(card);
            if (masterpiece <= 0 || card.CurrentUpgradeLevel < masterpiece)
            {
                continue;
            }

            int cardRef = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(card);
            if (!TriggeredCards.Add(cardRef))
            {
                continue;
            }

            System.Func<MegaCrit.Sts2.Core.Entities.Players.Player, CardModel> transformFactory = FrontierRules.GetMasterpieceTransformFactory(card);
            if (transformFactory == null)
            {
                GD.Print($"[Frontier] Masterpiece reached without transform mapping: {card.Id.Entry}");
                continue;
            }

            try
            {
                CardModel transformed = transformFactory(card.Owner);
                _ = CardCmd.Transform(card, transformed, CardPreviewStyle.HorizontalLayout);
                GD.Print($"[Frontier] Masterpiece transformed: {card.Id.Entry} -> {transformed.Id.Entry}");
            }
            catch (System.Exception e)
            {
                GD.Print($"[Frontier] Masterpiece transform failed: {card.Id.Entry} ({e.Message})");
            }
        }
    }
}

public sealed class HeatPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || !Owner.IsPlayer)
        {
            return;
        }

        if (Amount >= 70)
        {
            CardModel burn = CombatState.CreateCard<Burn>(Owner.Player);
            await CardPileCmd.Add(burn, PileType.Discard);
        }

        if (Amount >= 200)
        {
            decimal gainedBodyBurn = Amount / 100m;
            await PowerCmd.Apply<BodyBurnPower>(new[] { Owner }, gainedBodyBurn, Owner, null, false);
        }
    }
}

public sealed class BodyBurnPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (Amount <= 0 || cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }

        await CreatureCmd.Damage(context, Owner, Amount, ValueProp.Unpowered, null, cardPlay.Card);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player && Owner.IsPlayer)
        {
            await PowerCmd.Decrement(this);
        }
    }
}

