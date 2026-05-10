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

/// <summary>턴 시작 시 열기 임계 이상이면 열기 감소 + 드로우 (배기 시스템).</summary>
public sealed class ShumitExhaustSystemPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

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

/// <summary>턴 시작 시 화상을 손패에 (화염을 무서워하지 않는 자).</summary>
public sealed class ShumitFearlessFlamePower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromCard<Burn>() };

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player || Owner.CombatState is not CombatState combatState)
		{
			return;
		}

		CardModel burn = combatState.CreateCard<Burn>(player);
		await CardPileCmd.Add(burn, PileType.Hand);
	}
}

/// <summary>턴 동안 열기가 감소했으면 적 전체 피해 (증기 배출 — 단순화).</summary>
public sealed class ShumitSteamVentPower : CustomPowerModel
{
	/// <summary>열기 감소 판정 기준. 턴 시작 시 설정되며, 턴 중 첫 부여는 <see cref="AfterApplied"/>에서만 잡는다.</summary>
	private int _heatBaseline = int.MinValue;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => ShumitPowerKeywordHoverTips.Heat();

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (_heatBaseline == int.MinValue)
		{
			_heatBaseline = Owner.GetPower<HeatPower>()?.Amount ?? 0;
		}

		return base.AfterApplied(applier, cardSource);
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player)
		{
			return;
		}

		_heatBaseline = Owner.GetPower<HeatPower>()?.Amount ?? 0;
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != CombatSide.Player || !Owner.IsPlayer || CombatState == null)
		{
			return;
		}

		int now = Owner.GetPower<HeatPower>()?.Amount ?? 0;
		int baseline = _heatBaseline == int.MinValue ? now : _heatBaseline;
		if (now >= baseline)
		{
			return;
		}

		await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, Amount, ValueProp.Move, Owner, null);
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

		await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -Amount, Owner, null, silent: true);
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

/// <summary>플레이어가 손에서 화상을 플레이할 때마다 방어도 획득.</summary>
public sealed class ShumitFlameArmorPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromCard<Burn>(),
		HoverTipFactory.Static(StaticHoverTip.Block),
	};

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (!Owner.IsPlayer || Amount <= 0 || cardPlay.Card is not Burn)
		{
			return;
		}

		if (cardPlay.Card.Owner?.Creature != Owner)
		{
			return;
		}

		await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, cardPlay);
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

internal static class ShumitPowerKeywordHoverTips
{
	internal static IEnumerable<IHoverTip> Heat()
	{
		yield return HoverTipFactory.FromKeyword(FrontierKeywords.Heat);
	}
}

