using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Powers;

/// <summary>턴 종료 시 열기 감소 (냉각 시스템).</summary>
public sealed class ShumitCoolingSystemPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != CombatSide.Player || !Owner.IsPlayer)
		{
			return;
		}

		await FrontierHeatUtil.ReduceHeat(choiceContext, Owner, Amount, null);
	}
}

/// <summary>턴 종료 시 열기 증가 (뜨거워진 대장간).</summary>
public sealed class ShumitHeatedForgePower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != CombatSide.Player || !Owner.IsPlayer)
		{
			return;
		}

		await FrontierHeatUtil.ApplyHeat(choiceContext, Owner, Amount, null);
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

/// <summary>턴 시작 시 열기·힘 (뜨거운 노력). <see cref="CustomPowerModel.Amount"/>는 열기이며 힘은 열기 10당 1.</summary>
public sealed class ShumitFearlessFlamePower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips
		=> new IHoverTip[] { HoverTipFactory.FromKeyword(FrontierKeywords.Heat), HoverTipFactory.FromPower<StrengthPower>() };

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player)
		{
			return;
		}

		await FrontierHeatUtil.ApplyHeat(choiceContext, Owner, Amount, null);
		decimal str = Amount / 10m;
		if (str > 0m)
		{
			await PowerCmd.Apply<StrengthPower>(Owner, str, Owner, null, silent: false);
		}
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

/// <summary>턴 동안 턴 시작 시 획득 에너지에서 감소 (연마실).</summary>
public sealed class ShumitTurnEnergyPenaltyPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.ForEnergy(this) };

	public override decimal ModifyEnergyGain(Player player, decimal amount)
	{
		if (player != Owner.Player)
		{
			return amount;
		}

		return System.Math.Max(0m, amount - Amount);
	}

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

/// <summary>전투 중 화상/신체 화상 피해 면역(기획 의도). 실제 면역 훅은 후속 구현.</summary>
public sealed class ShumitHeartOfFlameImmunityPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromKeyword(FrontierKeywords.Heat),
		HoverTipFactory.FromKeyword(FrontierKeywords.BodyBurn),
		HoverTipFactory.FromCard<Burn>(),
	};
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

/// <summary>이번 턴 강화된 공격 사용 시 열기 보정 (금속 액화).</summary>
public sealed class ShumitUpgradedAttackBonusHeatPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

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
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == CombatSide.Player && Owner.IsPlayer)
		{
			await PowerCmd.Remove(this);
		}
	}
}

/// <summary>접쇠: 이번 턴 강화된 카드가 효과를 한 번 더 발동하는 횟수(슈미트: 재사용).</summary>
public sealed class FoldedSteelReplayPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.ReplayStatic) };

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (Amount <= 0 || card.Owner.Creature != Owner || card is FoldedSteelCard || card.CurrentUpgradeLevel <= 0)
		{
			return playCount;
		}

		return playCount + 1;
	}

	public override async Task AfterModifyingCardPlayCount(CardModel card)
	{
		await PowerCmd.Decrement(this);
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == Owner.Side)
		{
			await PowerCmd.Remove(this);
		}
	}
}

/// <summary>명인의 긍지 — 카드 강화 시 방어도.</summary>
public sealed class ShumitMasterPridePower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Block) };
}

/// <summary>목숨을 걸어 — 열기·신체 화상 감소 무효, 카드 사용마다 힘·민첩·열기(세션에 저장된 양).</summary>
public sealed class ShumitBetYourLifePower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

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

		int str = FrontierSession.GetBetYourLifeStrPerPlay(player);
		int dex = FrontierSession.GetBetYourLifeDexPerPlay(player);
		int heat = FrontierSession.GetBetYourLifeHeatPerPlay(player);
		if (str <= 0 && dex <= 0 && heat <= 0)
		{
			return;
		}

		if (str > 0)
		{
			await PowerCmd.Apply<StrengthPower>(Owner, str, Owner, cardPlay.Card, silent: false);
		}

		if (dex > 0)
		{
			await PowerCmd.Apply<DexterityPower>(Owner, dex, Owner, cardPlay.Card, silent: false);
		}

		if (heat > 0)
		{
			await FrontierHeatUtil.ApplyHeat(context, Owner, heat, cardPlay.Card);
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

