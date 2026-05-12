using Frontier.Cards;
using Godot.Bridge;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using Frontier.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using BaseLib.Patches.UI;
using BaseLib.Utils.Attributes;
using Frontier.Relics;
using System.Threading.Tasks;
using Frontier;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using Frontier.Powers;
using Frontier.Utilities;

/// <summary>
/// Frontier 모드 초기화 진입점.
/// 게임 로그의 <c>Asset not cached: res://...</c> WARN은 <see cref="MegaCrit.Sts2.Core.Assets.AssetCache"/>가
/// 프리로드 배치에 없던 리소스를 처음 로드할 때 출력하는 것으로, 로드 실패(ERROR)와 다르다.
/// 해당 WARN을 모드에서만 완전히 없애려면 이벤트/룸 단위 프리로드 목록을 코어에서 확장하는 패치가 필요하며 유지 비용이 크다.
/// </summary>
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
        RelicImageOverridePatch.AddOverride<HeatproofApronRelic>(icon);
        RelicImageOverridePatch.AddOverride<HephaestusBloodRelic>(icon);
        RelicImageOverridePatch.AddOverride<AncientAnvilRelic>(icon);
        RelicImageOverridePatch.AddOverride<FusionerHammerRelic>(icon);
        RelicImageOverridePatch.AddOverride<FusionerTongsRelic>(icon);
        RelicImageOverridePatch.AddOverride<FusionerAnvilRelic>(icon);
        RelicImageOverridePatch.AddOverride<EndlessLaborRelic>(icon);
        RelicImageOverridePatch.AddOverride<UnburnableBodyRelic>(icon);
        RelicImageOverridePatch.AddOverride<MasterpieceMuseumRelic>(icon);
        RelicImageOverridePatch.AddOverride<EternallyBurningFurnaceRelic>(icon);
    }
}

internal static class FrontierRules
{
    /// <summary><see cref="AnvilMemoryCard"/> 엔트리. 걸작(모루의 잔향)으로만 획득 — 보상·상점 <see cref="MegaCrit.Sts2.Core.Models.CardPoolModel.GetUnlockedCards"/> 및 전투 무작위 생성에서 제외.</summary>
    public const string AnvilMemoryCardEntry = "FRONTIER-ANVIL_MEMORY_CARD";

    /// <summary><see cref="FrenziedHeatCard"/> 엔트리. 걸작(멈출 수 없는 열기 +5)으로만 획득 — 보상·상점 및 전투 무작위 생성에서 제외.</summary>
    public const string FrenziedHeatCardEntry = "FRONTIER-FRENZIED_HEAT_CARD";

    /// <summary>
    /// <see cref="ShumitCharacter.StartingDeck"/> 카드(보상·상점 전용 풀에서 제외)와
    /// 전투 종료 인챈트(기민함/메아리/숙련/예리함 제련), 미구현 «불사르지 않는 몸» 등
    /// <see cref="ShumitCardPool.FilterThroughEpochs"/> 에서 걸러낼 엔트리.
    /// </summary>
    public static readonly HashSet<string> HiddenFromShumitCardPoolEntries = new(StringComparer.Ordinal)
    {
        "FRONTIER-STRIKE_SHUMIT_CARD",
        "FRONTIER-DEFEND_SHUMIT_CARD",
        "FRONTIER-FORGING_CARD",
        "FRONTIER-OIL_COOLING_CARD",
        "FRONTIER-AGILE_SMELTING_CARD",
        "FRONTIER-ECHO_SMELTING_CARD",
        "FRONTIER-MASTERY_SMELTING_CARD",
        "FRONTIER-SHARP_SMELTING_CARD",
        "FRONTIER-UNBURNING_BODY_CARD",
    };

    /// <summary>슈미트 «재련.»(숫자 없음) — 강화 상한 없음. <see cref="GetReforgeBonus"/>가 이 값을 반환하면 패치에서 <see cref="InfiniteUpgradeCap"/>으로 처리합니다.</summary>
    public const int ReforgeUnlimited = -1;
    public const int InfiniteUpgradeCap = 999;
    /// <summary>키는 <see cref="MegaCrit.Sts2.Core.Models.ModelDb.GetEntry"/> + PrefixIdPatch 이후의 <c>card.Id.Entry</c> (예: FRONTIER-BELLOWS_CARD).</summary>
    // 슈미트.md: «재련.»만 적힌 카드는 제한 없음. «재련 N»은 베이스 1에 +N.
    private static readonly Dictionary<string, int> ReforgeByCardId = new()
    {
        ["FRONTIER-BELLOWS_CARD"] = ReforgeUnlimited, // 풀무질 — 재련.
        [AnvilMemoryCardEntry] = ReforgeUnlimited, // 모루의 기억 — 재련.
        ["FRONTIER-BURNING_STRIKE_CARD"] = ReforgeUnlimited, // 불태우는 일격 — 재련.
        ["FRONTIER-SPARK_BURST_CARD"] = 9, // 불꽃 튀기기 — 재련 10 (베이스 1 + 보너스 9 = MaxUpgradeLevel 10).
        ["FRONTIER-STEAM_RELEASE_CARD"] = 10, // 증기 배출 — 재련 10.
        ["FRONTIER-REFINING_CARD"] = ReforgeUnlimited, // 정련 — 처치 시 영구 강화, 재련 제한 없음.
    };

    private static readonly Dictionary<string, int> MasterpieceByCardId = new()
    {
        ["FRONTIER-ANVIL_ECHO_CARD"] = 5, // 모루의 잔향 — 걸작 5.
        ["FRONTIER-UNSTOPPABLE_HEAT_CARD"] = 5, // 멈출 수 없는 열기 — 걸작 5.
    };

    /// <summary>두 번째 인자는 강화된 카드 — 비전투(휴식처 등)에서는 <see cref="CardModel.CardScope"/>가 런 스코프를 씁니다.</summary>
    private static readonly Dictionary<string, Func<Player, CardModel, CardModel>> MasterpieceTransformByCardId = new()
    {
        ["FRONTIER-ANVIL_ECHO_CARD"] = static (Player owner, CardModel source) =>
        {
            ICardScope? scope = source.CardScope;
            if (scope == null)
            {
                throw new InvalidOperationException("Anvil Echo masterpiece: CardScope is null.");
            }

            return scope.CreateCard<AnvilMemoryCard>(owner);
        },
        ["FRONTIER-UNSTOPPABLE_HEAT_CARD"] = static (Player owner, CardModel source) =>
        {
            ICardScope? scope = source.CardScope;
            if (scope == null)
            {
                throw new InvalidOperationException("Unstoppable Heat masterpiece: CardScope is null.");
            }

            return scope.CreateCard<FrenziedHeatCard>(owner);
        },
    };

    public static bool IsShumit(CardModel card)
    {
        if (card == null)
        {
            return false;
        }

        // Owner가 아직 세팅되지 않은 deserialize 단계(CardModel.FromSerializable)에서도 동작하도록
        // ShumitCard 인스턴스 자체를 우선 검사한다. 그렇지 않으면 MaxUpgradeLevel 패치가 미적용되어
        // 저장본의 강화 레벨 복원 시 «걸작·재련» 카드가 상한 초과로 throw할 수 있다.
        if (card is ShumitCard)
        {
            return true;
        }

        return card.Owner?.Character?.Id?.Entry == ShumitCharacter.CharacterId;
    }

    public static int GetReforgeBonus(CardModel card)
    {
        if (card?.Id?.Entry == null)
        {
            return 0;
        }

        int baseBonus = ReforgeByCardId.TryGetValue(card.Id.Entry, out int b) ? b : 0;

        if (baseBonus == ReforgeUnlimited)
        {
            return baseBonus;
        }

        if (!IsShumit(card))
        {
            return baseBonus;
        }

        // 슈미트 카드는 BrokenForgeRelic이 시작 렐릭이라 사실상 항상 보유 상태이다.
        // deserialize(card.Owner == null) 시점에는 RelicList 접근이 불가능하므로 안전하게 최소 보장만 적용한다.
        Player? owner = card.Owner;
        bool hasBrokenForge = owner == null || owner.GetRelic<BrokenForgeRelic>() != null;
        if (hasBrokenForge)
        {
            return Math.Max(baseBonus, 2);
        }

        return baseBonus;
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

    public static Func<Player, CardModel, CardModel>? GetMasterpieceTransformFactory(CardModel card)
    {
        string? entry = card?.Id?.Entry;
        if (entry == null)
        {
            return null;
        }

        return MasterpieceTransformByCardId.GetValueOrDefault(entry);
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
        // 슈미트 «재련.» — 무제한 강화.
        if (reforge == FrontierRules.ReforgeUnlimited)
        {
            __result = FrontierRules.InfiniteUpgradeCap;
            return;
        }

        // 걸작만 있는 카드(모루의 잔향): 최대 강화 = 걸작 단계(+5까지만). 베이스 1에 더하면 +6까지 되는 문제 방지.
        if (masterpiece > 0)
        {
            __result = masterpiece;
            return;
        }

        // 재련 보너스가 없으면 최대 강화 1.
        if (reforge <= 0)
        {
            __result = 1;
            return;
        }

        __result += reforge;
    }
}

[HarmonyPatch(typeof(CardCmd), "Upgrade", new[] { typeof(IEnumerable<CardModel>), typeof(CardPreviewStyle) })]
public static class FrontierMasterpieceTransformPatch
{
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

            Func<Player, CardModel, CardModel>? transformFactory = FrontierRules.GetMasterpieceTransformFactory(card);
            if (transformFactory == null)
            {
                GD.Print($"[Frontier] Masterpiece reached without transform mapping: {card.Id.Entry}");
                continue;
            }

            Player? owner = card.Owner;
            if (owner == null)
            {
                continue;
            }

            try
            {
                CardModel transformed = transformFactory(owner, card);
                // CardCmd.Upgrade는 동기 메서드라 이 Postfix도 동기 컨텍스트에서 실행된다.
                // CardCmd.Transform은 내부에 await Hook.* 호출이 다수 있는 async — 여기서 .GetAwaiter().GetResult()로
                // 기다리면 메인(게임) 스레드에서 await 컨티뉴를 받지 못해 데드락이 발생한다.
                // 코어 표준 fire-and-forget 패턴인 TaskHelper.RunSafely로 백그라운드 실행하고 예외는 로그로 남긴다.
                Task transformTask = CardCmd.Transform(card, transformed, CardPreviewStyle.HorizontalLayout);
                TaskHelper.RunSafely(transformTask);
                GD.Print($"[Frontier] Masterpiece transform scheduled: {card.Id.Entry} -> {transformed.Id.Entry}");
            }
            catch (Exception e)
            {
                GD.Print($"[Frontier] Masterpiece transform failed: {card.Id.Entry} ({e.Message})");
            }
        }
    }
}

[CustomID("FRONTIER-HEAT_POWER")]
public sealed class HeatPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<Burn>(),
        HoverTipFactory.FromKeyword(FrontierKeywords.BodyBurn),
    };

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || !Owner.IsPlayer)
        {
            return;
        }

        if (Amount >= 70)
        {
            Player? burnOwner = Owner.Player;
            if (burnOwner != null)
            {
                CardModel burn = FrontierCombatStateHelper.RequireFor(burnOwner).CreateCard<Burn>(burnOwner);
                await CardPileCmd.Add(burn, PileType.Discard);
            }
        }

        int bodyBurnThreshold = Owner.Player?.GetRelic<UnburnableBodyRelic>() != null ? 300 : 200;
        if (Amount >= bodyBurnThreshold)
        {
            decimal gainedBodyBurn = Amount / 100m;
            await PowerCmd.Apply<BodyBurnPower>(new[] { Owner }, gainedBodyBurn, Owner, null, false);
        }
    }
}

[CustomID("FRONTIER-BODY_BURN_POWER")]
public sealed class BodyBurnPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get { yield return HoverTipFactory.FromKeyword(FrontierKeywords.BodyBurn); }
    }

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
            if (ShumitBetYourLifePower.IsActive(Owner))
            {
                return;
            }

            await PowerCmd.Decrement(this);
        }
    }
}

