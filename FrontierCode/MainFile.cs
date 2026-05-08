using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using System.Collections.Generic;
using System.Linq;
using Frontier.Characters;
using Frontier.Cards.Rare;
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
using System.Threading.Tasks;

[ModInitializer("ModInit")]
public static class ModStart
{
    public static void ModInit()
    {
		Harmony harmony = new Harmony("sts2-frontier");
        harmony.PatchAll();

        GD.Print("[Frontier] Mod Initialized");
    }
}

internal static class FrontierRules
{
    // Reforge/Masterpiece 수치가 0 이하이면 "무한 강화" 취급.
    public const int InfiniteUpgradeCap = 999;
    private static readonly Dictionary<string, int> ReforgeByCardId = new()
    {
        // "재련." 표기는 기본 1로 처리
        ["BellowsCard"] = 1,
        ["AnvilMemoryCard"] = 1,
        ["BurningStrikeCard"] = 1,
        // 수치가 명시된 재련
        ["SparkBurstCard"] = 5,
        ["SteamReleaseCard"] = 10
    };

    private static readonly Dictionary<string, int> MasterpieceByCardId = new()
    {
        ["AnvilEchoCard"] = 10
    };
    private static readonly Dictionary<string, System.Func<MegaCrit.Sts2.Core.Entities.Players.Player, CardModel>> MasterpieceTransformByCardId = new()
    {
        ["AnvilEchoCard"] = static (MegaCrit.Sts2.Core.Entities.Players.Player owner) => owner.RunState.CreateCard<AnvilMemoryCard>(owner)
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
        if (reforge <= 0 && masterpiece <= 0)
        {
            __result = FrontierRules.InfiniteUpgradeCap;
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

    public override List<(string, string)> Localization => new()
    {
        ("title", "열기"),
        ("description", "열기가 70 이상이면 턴 종료 시 버린 카드 더미에 화상 1장을 추가합니다. 열기가 200 이상이면 턴 종료 시 열기 100마다 신체 화상을 1 얻습니다.")
    };

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
            int gainedBodyBurn = Amount / 100;
            await PowerCmd.Apply<BodyBurnPower>(new[] { Owner }, gainedBodyBurn, Owner, null, false);
        }
    }
}

public sealed class BodyBurnPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new()
    {
        ("title", "신체 화상"),
        ("description", "카드를 사용할 때마다 신체 화상 수치만큼 피해를 받습니다. 턴 종료 시 1 감소합니다.")
    };

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
