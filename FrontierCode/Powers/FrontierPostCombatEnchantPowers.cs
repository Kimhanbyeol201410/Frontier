using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Rooms;

namespace Frontier.Powers;

public sealed class PostCombatSharpEnchantPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Sharp>();

	public override async Task AfterCombatVictory(CombatRoom room)
	{
		Player? p = Owner.Player;
		if (p == null)
		{
			await PowerCmd.Remove(this);
			return;
		}

		EnchantmentModel ench = ModelDb.Enchantment<Sharp>().ToMutable();
		CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, 1);
		IEnumerable<CardModel>? sel = await CardSelectCmd.FromDeckForEnchantment(p, ench, 1, prefs);
		if (sel != null)
		{
			foreach (CardModel c in sel)
			{
				CardCmd.Enchant<Sharp>(c, Amount);
			}
		}

		await PowerCmd.Remove(this);
	}
}

public sealed class PostCombatNimbleEnchantPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Nimble>();

	public override async Task AfterCombatVictory(CombatRoom room)
	{
		Player? p = Owner.Player;
		if (p == null)
		{
			await PowerCmd.Remove(this);
			return;
		}

		EnchantmentModel ench = ModelDb.Enchantment<Nimble>().ToMutable();
		CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, 1);
		IEnumerable<CardModel>? sel = await CardSelectCmd.FromDeckForEnchantment(p, ench, 1, prefs);
		if (sel != null)
		{
			foreach (CardModel c in sel)
			{
				CardCmd.Enchant<Nimble>(c, Amount);
			}
		}

		await PowerCmd.Remove(this);
	}
}

public sealed class PostCombatAdroitEnchantPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Adroit>();

	public override async Task AfterCombatVictory(CombatRoom room)
	{
		Player? p = Owner.Player;
		if (p == null)
		{
			await PowerCmd.Remove(this);
			return;
		}

		EnchantmentModel ench = ModelDb.Enchantment<Adroit>().ToMutable();
		CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, 1);
		IEnumerable<CardModel>? sel = await CardSelectCmd.FromDeckForEnchantment(p, ench, 1, prefs);
		if (sel != null)
		{
			foreach (CardModel c in sel)
			{
				CardCmd.Enchant<Adroit>(c, Amount);
			}
		}

		await PowerCmd.Remove(this);
	}
}

public sealed class PostCombatSpiralEnchantPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Spiral>();

	public override async Task AfterCombatVictory(CombatRoom room)
	{
		Player? p = Owner.Player;
		if (p == null)
		{
			await PowerCmd.Remove(this);
			return;
		}

		EnchantmentModel ench = ModelDb.Enchantment<Spiral>().ToMutable();
		CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, 1);
		IEnumerable<CardModel>? sel = await CardSelectCmd.FromDeckForEnchantment(
			p,
			ench,
			1,
			static (CardModel? c) => c != null && c.Rarity == CardRarity.Basic && (c.Tags.Any((CardTag t) => t == CardTag.Strike) || c.Tags.Any((CardTag t) => t == CardTag.Defend)),
			prefs);
		if (sel != null)
		{
			foreach (CardModel c in sel)
			{
				CardCmd.Enchant<Spiral>(c, Amount);
			}
		}

		await PowerCmd.Remove(this);
	}
}
