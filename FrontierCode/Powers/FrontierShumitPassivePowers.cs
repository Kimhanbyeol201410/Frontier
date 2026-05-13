using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Frontier.Cards;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Powers;

/// <summary>턴 종료 시 열기 감소 + 턴 시작 시 방어도 (냉각 시스템).</summary>
public sealed class ShumitCoolingSystemPower : CustomPowerModel
{
	/// <summary>턴 시작 시 부여하는 방어도. 카드 강화 레벨에 따라 카드에서 <see cref="SetBlockPerTurn"/> 으로 갱신된다.</summary>
	private const string BlockPerTurnKey = "BlockPerTurn";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new[]
	{
		new DynamicVar(BlockPerTurnKey, 5m),
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	/// <summary>카드 인스턴스의 «BlockPerTurn» 강화 값을 power 에 동기화. 중복 사용 시 더 큰 값을 유지한다.</summary>
	public void SetBlockPerTurn(decimal value)
	{
		AssertMutable();
		DynamicVar var = DynamicVars[BlockPerTurnKey];
		if (value > var.BaseValue)
		{
			var.BaseValue = value;
		}
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player || !Owner.IsPlayer)
		{
			return;
		}

		await FrontierHeatUtil.ReduceHeat(choiceContext, Owner, Amount, null);
		await CreatureCmd.GainBlock(Owner, DynamicVars[BlockPerTurnKey].BaseValue, ValueProp.Move, null);
	}
}

/// <summary>뜨거워진 대장간: 매 턴 종료 시 현재 열기 ÷ <see cref="CustomPowerModel.Amount"/>(분모) 만큼 체력을 회복.
/// <para>여러 장 사용 시 분모가 누적되면 회복량이 오히려 감소하므로 <see cref="IsInstanced"/>=true 로 각 카드를 별도 인스턴스로 동작시킨다.</para>
/// </summary>
public sealed class ShumitHeatedForgePower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool IsInstanced => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != CombatSide.Player || !Owner.IsPlayer)
		{
			return;
		}

		int heat = Owner.GetPower<HeatPower>()?.Amount ?? 0;
		int divisor = System.Math.Max(1, Amount);
		int healAmount = heat / divisor;
		if (healAmount > 0)
		{
			await CreatureCmd.Heal(Owner, healAmount);
		}
	}
}

/// <summary>턴 시작 시 열기 임계 이상이면 열기 감소 + 드로우 (배기 시스템). 카드마다 별도 인스턴스(임계값 독립).</summary>
public sealed class ShumitExhaustSystemPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool IsInstanced => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player)
		{
			return;
		}

		int heat = Owner.GetPower<HeatPower>()?.Amount ?? 0;
		if (heat < Amount)
		{
			return;
		}

		await FrontierHeatUtil.ReduceHeat(choiceContext, Owner, 10m, null);
		await CardPileCmd.Draw(choiceContext, 1, player);
	}
}

/// <summary>머리 식히기: 다음 내 턴 시작 시 카드 1장 드로우 후 열기 <see cref="CustomPowerModel.Amount"/>만큼 감소하고 파워 제거.</summary>
[CustomID("FRONTIER-SHUMIT_COOL_HEAD_NEXT_TURN_POWER")]
public sealed class ShumitCoolHeadNextTurnPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player || !Owner.IsPlayer || Amount <= 0m)
		{
			return;
		}

		await CardPileCmd.Draw(choiceContext, 1, player);
		await FrontierHeatUtil.ReduceHeat(choiceContext, Owner, Amount, null);
		await PowerCmd.Remove(this);
	}
}

/// <summary>환기: 이번 턴에 카드를 사용할 때마다 열기 <see cref="CustomPowerModel.Amount"/>만큼 감소 + 방어도 <see cref="BlockPerCard"/> 획득. 턴 종료 시 제거. 카드마다 별도 인스턴스(여러 장 중첩 가능).</summary>
[CustomID("FRONTIER-SHUMIT_VENTILATION_THIS_TURN_POWER")]
public sealed class ShumitVentilationThisTurnPower : CustomPowerModel
{
	/// <summary>환기 발동 시 카드 사용마다 부여하는 방어도. <see cref="IsInstanced"/>=true 이므로 각 인스턴스가 자신만의 값을 가지며, 카드(강화 단계) 측에서 적용 직후 덮어 설정한다.</summary>
	public int BlockPerCard { get; set; } = 3;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool IsInstanced => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (Amount <= 0m || cardPlay.Card?.Owner?.Creature != Owner)
		{
			return;
		}

		await FrontierHeatUtil.ReduceHeat(context, Owner, Amount, cardPlay.Card);
		if (BlockPerCard > 0)
		{
			await CreatureCmd.GainBlock(Owner, BlockPerCard, ValueProp.Move, cardPlay);
		}
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == CombatSide.Player && Owner.IsPlayer)
		{
			await PowerCmd.Remove(this);
		}
	}
}

/// <summary>연료 최대로: 다음 내 턴 시작 시 에너지 <see cref="CustomPowerModel.Amount"/>를 얻고 파워 제거.</summary>
[CustomID("FRONTIER-SHUMIT_FUEL_MAX_NEXT_TURN_ENERGY_POWER")]
public sealed class ShumitFuelMaxNextTurnEnergyPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.ForEnergy(this) };

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player || !Owner.IsPlayer || Amount <= 0m)
		{
			return;
		}

		int energy = (int)Amount;
		if (energy > 0)
		{
			await PlayerCmd.GainEnergy(energy, player);
		}

		await PowerCmd.Remove(this);
	}
}

/// <summary>턴 시작 시 열기 부여 (뜨거운 노력). <see cref="CustomPowerModel.Amount"/>는 매 턴 부여 열기.
/// 힘은 카드 사용 시점에 카드의 <c>StrGain</c> 값만큼 1회만 즉시 부여하므로 본 파워에서는 처리하지 않는다.</summary>
public sealed class ShumitFearlessFlamePower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips
		=> new IHoverTip[] { HoverTipFactory.FromKeyword(FrontierKeywords.Heat) };

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player)
		{
			return;
		}

		await FrontierHeatUtil.ApplyHeat(choiceContext, Owner, Amount, null);
	}
}

/// <summary>열기가 감소할 때마다 적 전체 피해 (증기 배출).</summary>
public sealed class ShumitSteamVentPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (amount >= 0m || power.Owner != Owner || !Owner.IsPlayer || power.Id.Entry != "FRONTIER-HEAT_POWER")
		{
			return;
		}

		if (FrontierCombatStateHelper.TryGetFor(Owner.Player) is not CombatState combatState)
		{
			return;
		}

		await CreatureCmd.Damage(
			new BlockingPlayerChoiceContext(),
			combatState.HittableEnemies,
			Amount,
			ValueProp.Move,
			Owner,
			null);
	}
}

/// <summary>턴 종료 시 이번 카드로 부여한 힘을 제거 (다가가는 공포 등 임시 힘).</summary>
public sealed class ShumitStripStrengthAtTurnEndPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<StrengthPower>() };

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != CombatSide.Player || !Owner.IsPlayer || Amount <= 0)
		{
			return;
		}

		await PowerCmd.Apply<StrengthPower>(Owner, -Amount, Owner, null, silent: true);
		await PowerCmd.Remove(this);
	}
}

/// <summary>턴 종료 시 이번 카드로 부여한 민첩을 제거 (리버스 엔지니어링 등 임시 민첩).</summary>
[CustomID("FRONTIER-SHUMIT_STRIP_DEXTERITY_AT_TURN_END_POWER")]
public sealed class ShumitStripDexterityAtTurnEndPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<DexterityPower>() };

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != CombatSide.Player || !Owner.IsPlayer || Amount <= 0)
		{
			return;
		}

		await PowerCmd.Apply<DexterityPower>(Owner, -Amount, Owner, null, silent: true);
		await PowerCmd.Remove(this);
	}
}

/// <summary>이번 턴 [열기] 증가·감소 방향이 뒤바뀜 (리버스 엔지니어링). 모드 패치가 <c>PowerCmd.Apply&lt;HeatPower&gt;</c> 적용량 부호를 반전합니다.</summary>
[CustomID("FRONTIER-SHUMIT_REVERSE_ENGINEERING_INVERT_HEAT_POWER")]
public sealed class ShumitReverseEngineeringInvertHeatPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == CombatSide.Player && Owner.IsPlayer)
		{
			await PowerCmd.Remove(this);
		}
	}
}

/// <summary>플레이어 소유 [화염](Burn) 카드가 전투 더미에 새로 들어올 때마다(생성 직후 첫 입더미) 방어도 획득.</summary>
public sealed class ShumitFlameArmorPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromCard<Burn>(),
		HoverTipFactory.Static(StaticHoverTip.Block),
	};

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		if (!Owner.IsPlayer || Amount <= 0 || oldPileType != PileType.None || card is not Burn)
		{
			return;
		}

		if (card.Owner?.Creature != Owner)
		{
			return;
		}

		await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
	}
}

/// <summary>화염의 심장: 플레이어 소유 [화상](Burn) 카드가 전투 더미에 새로 들어올 때마다 에너지 <see cref="CustomPowerModel.Amount"/> 획득.</summary>
[CustomID("FRONTIER-SHUMIT_HEART_OF_FLAME_ENERGY_POWER")]
public sealed class ShumitHeartOfFlameEnergyPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromCard<Burn>(),
	};

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		if (!Owner.IsPlayer || Amount <= 0 || card is not Burn)
		{
			return;
		}

		if (card.Owner?.Creature != Owner)
		{
			return;
		}

		Player? player = Owner.Player;
		if (player == null)
		{
			return;
		}

		await PlayerCmd.GainEnergy(Amount, player);
	}
}

/// <summary>신의 형상: 내 턴 시작 시 손패의 강화 가능한 모든 카드를 1회 강화. <see cref="CustomPowerModel.Amount"/> 는 보유 표시용 카운터.</summary>
[CustomID("FRONTIER-SHUMIT_DIVINE_FORM_POWER")]
public sealed class ShumitDivineFormPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player || !Owner.IsPlayer || Amount <= 0)
		{
			return Task.CompletedTask;
		}

		List<CardModel> toUpgrade = PileType.Hand.GetPile(player).Cards.Where(static c => c is { IsUpgradable: true }).ToList();
		if (toUpgrade.Count == 0)
		{
			return Task.CompletedTask;
		}

		CardCmd.Upgrade(toUpgrade, CardPreviewStyle.HorizontalLayout);
		return Task.CompletedTask;
	}
}

/// <summary>다음으로 사용하는 공격에 열기 부가 (제련 설계).</summary>
public sealed class ShumitNextAttackHeatPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (Amount <= 0 || cardPlay.Card?.Owner?.Creature != Owner || cardPlay.Card.Type != CardType.Attack)
		{
			return;
		}

		await FrontierHeatUtil.ApplyHeat(context, Owner, Amount, cardPlay.Card);
		await PowerCmd.Remove(this);
	}
}

/// <summary>금속 액화: 이번 턴 강화된 공격 카드 비용 0, 사용 시마다 열기·뽑을 더미에 화상 1장.</summary>
public sealed class ShumitUpgradedAttackBonusHeatPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	/// <summary>동일 턴 중 카드 여러 장 사용 시 중복 버프 방지.</summary>
	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (card?.Owner?.Creature != Owner || !Owner.IsPlayer)
		{
			return false;
		}

		if (card.Type != CardType.Attack || card.CurrentUpgradeLevel <= 0)
		{
			return false;
		}

		switch (card.Pile?.Type)
		{
			case PileType.Hand:
			case PileType.Play:
				modifiedCost = 0m;
				return true;
			default:
				return false;
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card?.Owner?.Creature != Owner || cardPlay.Card.Type != CardType.Attack)
		{
			return;
		}

		if (cardPlay.Card.CurrentUpgradeLevel <= 0)
		{
			return;
		}

		await FrontierHeatUtil.ApplyHeat(context, Owner, Amount, cardPlay.Card);

		Player? player = Owner.Player;
		if (player == null || FrontierCombatStateHelper.TryGetFor(player) is not CombatState combatState)
		{
			return;
		}

		CardModel burn = combatState.CreateCard<Burn>(player);
		await CardPileCmd.Add(burn, PileType.Draw, CardPilePosition.Random, cardPlay.Card);
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == CombatSide.Player && Owner.IsPlayer)
		{
			await PowerCmd.Remove(this);
		}
	}
}

/// <summary>접쇠: 이번 턴 「다음 강화된 카드 1장」이 효과를 «Amount»번 추가 발동(슈미트: 재사용). 추가 재사용 1회당 <see cref="HeatPerReplay"/> 열기 획득.</summary>
public sealed class FoldedSteelReplayPower : CustomPowerModel
{
	/// <summary>추가 재사용 1회당 부여하는 열기. 자격 조건(소유자/강화 카드/접쇠 자체 제외)을 충족해 ModifyCardPlayCount 가 실제로 카운트를 증가시킨 경우에만 적용된다.</summary>
	private const int HeatPerReplay = 15;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.ReplayStatic) };

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (Amount <= 0 || card.Owner.Creature != Owner || card is FoldedSteelCard || card.CurrentUpgradeLevel <= 0)
		{
			return playCount;
		}

		return playCount + Amount;
	}

	public override async Task AfterModifyingCardPlayCount(CardModel card)
	{
		// 이 hook은 ModifyCardPlayCount 가 실제로 카운트를 변경한 modifier 에 대해서만 호출됨(Hook.cs).
		// 따라서 Amount 가 그대로 추가 재사용 횟수와 동일하다.
		int gain = Amount * HeatPerReplay;
		if (gain > 0)
		{
			await PowerCmd.Apply<HeatPower>(Owner, gain, Owner, null);
		}
		await PowerCmd.Remove(this);
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == Owner.Side)
		{
			await PowerCmd.Remove(this);
		}
	}
}

/// <summary>명인의 긍지 — 카드 강화 시 방어도 <see cref="CustomPowerModel.Amount"/> 획득.</summary>
public sealed class ShumitMasterPridePower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Block) };
}

/// <summary>목숨을 걸어 — 열기·신체 화상 감소 무효, 카드 타입별 보너스(스킬=힘, 공격=민첩). <see cref="CustomPowerModel.Amount"/> 는 보유 표시용 카운터.</summary>
public sealed class ShumitBetYourLifePower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public static bool IsActive(Creature? creature)
		=> creature?.GetPower<ShumitBetYourLifePower>() != null;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromKeyword(FrontierKeywords.Heat),
		HoverTipFactory.FromKeyword(FrontierKeywords.BodyBurn),
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.FromPower<DexterityPower>(),
	};

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card?.Owner?.Creature != Owner)
		{
			return;
		}

		Player? player = Owner.Player;
		if (player == null)
		{
			return;
		}

		switch (cardPlay.Card.Type)
		{
			case CardType.Skill:
			{
				int str = FrontierSession.GetBetYourLifeStrPerPlay(player);
				if (str > 0)
				{
					await PowerCmd.Apply<StrengthPower>(Owner, str, Owner, cardPlay.Card, silent: false);
				}
				break;
			}
			case CardType.Attack:
			{
				int dex = FrontierSession.GetBetYourLifeDexPerPlay(player);
				if (dex > 0)
				{
					await PowerCmd.Apply<DexterityPower>(Owner, dex, Owner, cardPlay.Card, silent: false);
				}
				break;
			}
		}
	}
}

internal static class ShumitPowerKeywordHoverTips
{
	internal static IEnumerable<IHoverTip> Heat()
	{
		yield return HoverTipFactory.FromKeyword(FrontierKeywords.Heat);
	}
}

/// <summary>
/// 무수히 많은 기억 — X 턴 동안 매 턴 시작 시 에너지 <see cref="EnergyPerTurn"/> 획득 + 카드 <see cref="DrawPerTurn"/> 장 추가 드로우.
/// <para>스택 타입은 <see cref="PowerStackType.Counter"/>. 매 턴 시작에 효과를 적용한 뒤 1 씩 감소시키며,
/// 1 이하에 도달하면 마지막 발동 후 자체 제거된다.</para>
/// </summary>
public sealed class CountlessMemoriesPower : CustomPowerModel
{
	public const int EnergyPerTurn = 2;
	public const int DrawPerTurn = 4;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.ForEnergy(this),
	};

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player || !Owner.IsPlayer || Amount <= 0)
		{
			return;
		}

		await PlayerCmd.GainEnergy(EnergyPerTurn, player);
		await CardPileCmd.Draw(choiceContext, DrawPerTurn, player);

		if (Amount > 1)
		{
			await PowerCmd.Apply<CountlessMemoriesPower>(Owner, -1, Owner, null, silent: true);
		}
		else
		{
			await PowerCmd.Remove(this);
		}
	}
}

