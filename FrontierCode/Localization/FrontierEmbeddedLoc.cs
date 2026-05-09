using System;
using System.Collections.Generic;
using BaseLib.Extensions;
using BaseLib.Patches.Localization;
using Frontier.Relics;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;

namespace Frontier.Localization;

/// <summary>
/// DLL-only 모드에서는 <c>res://{manifest-id}/localization/...</c>가 마운트되지 않아 JSON 로캘이 합쳐지지 않는다.
/// BaseLib ILocalizationProvider(ModelLocPatch) 경로와 별도로,
/// <see cref="LocManager"/>가 <see cref="ModelDb.Init"/>보다 먼저 뜨는 타이밍·NuGet BaseLib 차이를 흡수하기 위해
/// <see cref="LocTable.MergeWith"/>로도 주입한다.
/// </summary>
public static class FrontierEmbeddedLoc
{
	/// <summary>언어 변경 시마다 호출해 characters/relics 테이블에 모드 키를 합친다.</summary>
	public static void ApplyToLocManager(LocManager loc, string language)
	{
		if (loc == null || string.IsNullOrEmpty(language))
		{
			return;
		}

		try
		{
			bool kor = language == "kor";
			Dictionary<string, string> characters = new();
			foreach ((string field, string text) in kor ? ShumitCharacterKor : ShumitCharacterEng)
			{
				characters[global::Frontier.Characters.ShumitCharacter.CharacterId + "." + field] = SafeTrySimplify(text);
			}

			Dictionary<string, string> relics = new();
			foreach (KeyValuePair<System.Type, RelicLocPair> kv in RelicLocByType)
			{
				// ModelDb.GetEntry 패치 전/후 타이밍에 의존하지 않도록 PrefixIdPatch 와 동일 규칙으로 Entry 계산
				string entry = kv.Key.GetPrefix() + StringHelper.Slugify(kv.Key.Name);
				List<(string field, string text)>? rows = kor ? kv.Value.Kor : kv.Value.Eng;
				if (rows == null)
				{
					continue;
				}

				foreach ((string field, string text) in rows)
				{
					relics[entry + "." + field] = SafeTrySimplify(text);
				}
			}

			TryMergeLocTable(loc, "characters", characters);
			TryMergeLocTable(loc, "relics", relics);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[Frontier] ApplyToLocManager failed: {ex}");
		}
	}

	/// <summary>
	/// <see cref="LocManager.GetTable"/>는 키가 있어도 값이 null이면 그대로 null을 돌려줄 수 있어 <see cref="LocTable.MergeWith"/>에서 NRE가 난다.
	/// </summary>
	private static void TryMergeLocTable(LocManager loc, string tableName, Dictionary<string, string> rows)
	{
		if (rows == null || rows.Count == 0)
		{
			return;
		}

		try
		{
			LocTable table = loc.GetTable(tableName);
			if (table == null)
			{
				GD.PrintErr($"[Frontier] LocTable '{tableName}' was null; skip embedded loc merge.");
				return;
			}

			table.MergeWith(rows);
		}
		catch (LocException ex)
		{
			GD.PrintErr($"[Frontier] Cannot merge into '{tableName}': {ex.Message}");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[Frontier] MergeInto '{tableName}' failed: {ex}");
		}
	}

	/// <summary>
	/// <see cref="SimpleLoc.TrySimplify"/> 내부 <c>Simplify</c> 가 <c>BaseLibMain.Logger.Info</c> 를 호출한다.
	/// LocManager 초기화 초반에는 Logger 가 아직 null 이라 NRE 가 날 수 있다.
	/// </summary>
	private static string SafeTrySimplify(string? raw)
	{
		string s = raw ?? "";
		if (s.Length == 0)
		{
			return s;
		}

		try
		{
			return FrontierLocJsonEscapes.Normalize(SimpleLoc.TrySimplify(s));
		}
		catch
		{
			return FrontierLocJsonEscapes.Normalize(s);
		}
	}

	public static List<(string, string)>? ShumitCharacter()
	{
		return IsKorean() ? ShumitCharacterKor : ShumitCharacterEng;
	}

	public static List<(string, string)>? ForRelic(System.Type relicType)
	{
		if (RelicLocByType.TryGetValue(relicType, out var pair))
		{
			return IsKorean() ? pair.Kor : pair.Eng;
		}

		return null;
	}

	private static bool IsKorean()
	{
		try
		{
			return LocManager.Instance?.Language == "kor";
		}
		catch
		{
			return false;
		}
	}

	private sealed record RelicLocPair(List<(string, string)> Eng, List<(string, string)> Kor);

	private static readonly Dictionary<System.Type, RelicLocPair> RelicLocByType = new()
	{
		[typeof(BrokenForgeRelic)] = new RelicLocPair(BrokenForgeEng, BrokenForgeKor),
		[typeof(GreatForgeRelic)] = new RelicLocPair(GreatForgeEng, GreatForgeKor),
		[typeof(HeatproofApronRelic)] = new RelicLocPair(HeatproofApronEng, HeatproofApronKor),
		[typeof(SmelterShardRelic)] = new RelicLocPair(SmelterShardEng, SmelterShardKor),
		[typeof(HeartOfFlameRelic)] = new RelicLocPair(HeartOfFlameEng, HeartOfFlameKor),
		[typeof(BlastFurnaceShardRelic)] = new RelicLocPair(BlastFurnaceShardEng, BlastFurnaceShardKor),
		[typeof(GrindingRoomShardRelic)] = new RelicLocPair(GrindingRoomShardEng, GrindingRoomShardKor),
		[typeof(HephaestusBloodRelic)] = new RelicLocPair(HephaestusBloodEng, HephaestusBloodKor),
		[typeof(AncientAnvilRelic)] = new RelicLocPair(AncientAnvilEng, AncientAnvilKor),
		[typeof(FusionerHammerRelic)] = new RelicLocPair(FusionerHammerEng, FusionerHammerKor),
		[typeof(FusionerTongsRelic)] = new RelicLocPair(FusionerTongsEng, FusionerTongsKor),
		[typeof(FusionerAnvilRelic)] = new RelicLocPair(FusionerAnvilEng, FusionerAnvilKor)
	};

	private static readonly List<(string, string)> ShumitCharacterEng = new()
	{
		("title", "Shumit"),
		("titleObject", "Shumit"),
		("banter.alive.endTurnPing", "One more strike."),
		("banter.dead.endTurnPing", "...Not done yet."),
		("description", "One of the pioneers dragged into another world. He raises his hammer and carves a path to return to his home world. By trade, he is a blacksmith who forges gear for the battlefield."),
		("aromaPrinciple", "[sine][orange]Steel remembers every strike.[/orange][/sine]"),
		("cardsModifierDescription", "Shumit cards will now appear in rewards and shops."),
		("cardsModifierTitle", "Shumit Cards"),
		("eventDeathPrevention", "Shumit endured this trial."),
		("goldMonologue", "[sine]Enough iron, and I can forge a road home.[/sine]"),
		("possessiveAdjective", "his"),
		("pronounObject", "him"),
		("pronounPossessive", "his"),
		("pronounSubject", "he"),
		("unlockText", "A blacksmith from another world, forging his way home.")
	};

	private static readonly List<(string, string)> ShumitCharacterKor = new()
	{
		("title", "슈미트"),
		("titleObject", "슈미트"),
		("banter.alive.endTurnPing", "한 번 더 벼린다."),
		("banter.dead.endTurnPing", "...아직 끝나지 않았다."),
		("description", "이세계로 끌려온 개척자 중 한 명. 원래 세계로 돌아가기 위해 망치를 들고 길을 연다. 그의 본업은 전장의 장비를 벼려내는 대장장이다."),
		("aromaPrinciple", "[sine][orange]강철은 모든 타격을 기억한다.[/orange][/sine]"),
		("cardsModifierDescription", "슈미트 카드가 보상과 상점에 등장합니다."),
		("cardsModifierTitle", "슈미트 카드"),
		("eventDeathPrevention", "슈미트는 이를 버텨냈다."),
		("goldMonologue", "[sine]철만 충분하다면, 돌아갈 길도 벼려낼 수 있다.[/sine]"),
		("possessiveAdjective", "그의"),
		("pronounObject", "그"),
		("pronounPossessive", "그의"),
		("pronounSubject", "그"),
		("unlockText", "망치로 길을 여는 이세계의 대장장이")
	};

	private static List<(string, string)> R(string title, string description) => new() { ("title", title), ("description", description) };

	private static readonly List<(string, string)> BrokenForgeEng = R("Broken Forge",
		"At combat start, the forge mechanism activates. You draw fewer cards, and each turn you manually upgrade 1 card in your hand.");

	private static readonly List<(string, string)> BrokenForgeKor = R("부서진 대장간",
		"전투 시작 시 대장간 장치가 가동됩니다. 드로우가 감소하며, 매 턴 손패 카드 1장을 직접 선택해 강화합니다.");

	private static readonly List<(string, string)> GreatForgeEng = R("Great Forge", "At combat start, add 1 [gold]Great Forge[/gold] to your draw pile.");
	private static readonly List<(string, string)> GreatForgeKor = R("위대한 대장간", "전투 시작 시 뽑을 카드 더미에 [gold]위대한 대장간[/gold] 1장을 추가합니다.");

	private static readonly List<(string, string)> HeatproofApronEng = R("Heatproof Leather Apron", "Whenever you gain Heat, gain 2 Block.");
	private static readonly List<(string, string)> HeatproofApronKor = R("내열 가죽 앞치마", "열기를 얻을 때마다 방어도 2를 얻습니다.");

	private static readonly List<(string, string)> SmelterShardEng = R("Smelter Shard", "At combat start, add 1 [gold]Smelter[/gold] to your draw pile.");
	private static readonly List<(string, string)> SmelterShardKor = R("제련소의 파편", "전투 시작 시 뽑을 카드 더미에 [gold]제련소[/gold] 1장을 추가합니다.");

	private static readonly List<(string, string)> HeartOfFlameEng = R("Heart of Flame", "Burn/Body Burn mitigation effect is planned in a future update.");
	private static readonly List<(string, string)> HeartOfFlameKor = R("화염의 심장", "화상/신체 화상 관련 효과는 추후 구현 예정입니다.");

	private static readonly List<(string, string)> BlastFurnaceShardEng = R("Blast Furnace Shard", "At combat start, add 1 [gold]Blast Furnace[/gold] to your draw pile.");
	private static readonly List<(string, string)> BlastFurnaceShardKor = R("용광로의 파편", "전투 시작 시 뽑을 카드 더미에 [gold]용광로[/gold] 1장을 추가합니다.");

	private static readonly List<(string, string)> GrindingRoomShardEng = R("Grinding Room Shard", "At combat start, add 1 [gold]Grinding Room[/gold] to your draw pile.");
	private static readonly List<(string, string)> GrindingRoomShardKor = R("연마실의 파편", "전투 시작 시 뽑을 카드 더미에 [gold]연마실[/gold] 1장을 추가합니다.");

	private static readonly List<(string, string)> HephaestusBloodEng = R("Blood of Hephaestus", "Gain 1 Strength for every 20 Heat.");
	private static readonly List<(string, string)> HephaestusBloodKor = R("헤파이스토스의 피", "열기 20마다 힘 1을 얻습니다.");

	private static readonly List<(string, string)> AncientAnvilEng = R("Ancient Anvil", "Whenever you play an upgraded card, gain 3 Block.");
	private static readonly List<(string, string)> AncientAnvilKor = R("오래된 모루", "강화된 카드를 사용할 때마다 방어도 3을 얻습니다.");

	private static readonly List<(string, string)> FusionerHammerEng = R("Fusioner Hammer", "Enchant-on-obtain effect is planned in a future update.");
	private static readonly List<(string, string)> FusionerHammerKor = R("융합자의 망치", "인챈트 효과는 추후 구현 예정입니다.");

	private static readonly List<(string, string)> FusionerTongsEng = R("Fusioner Tongs", "Whenever you play an upgraded card, gain 1 Strength.");
	private static readonly List<(string, string)> FusionerTongsKor = R("융합자의 집게", "강화된 카드를 사용할 때마다 힘 1을 얻습니다.");

	private static readonly List<(string, string)> FusionerAnvilEng = R("Fusioner Anvil", "Whenever you play an upgraded card, gain 5 Block.");
	private static readonly List<(string, string)> FusionerAnvilKor = R("융합자의 모루", "강화된 카드를 사용할 때마다 방어도 5를 얻습니다.");
}
