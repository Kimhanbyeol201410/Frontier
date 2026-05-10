using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Utilities;

internal static class FrontierHeatUtil
{
	internal static async System.Threading.Tasks.Task ApplyHeat(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source)
	{
		if (amount == 0m || creature == null)
		{
			return;
		}

		await PowerCmd.Apply<HeatPower>(choiceContext, creature, amount, creature, source);
	}

	internal static async System.Threading.Tasks.Task ReduceHeat(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source)
	{
		if (amount == 0m || creature == null)
		{
			return;
		}

		int current = creature.GetPower<HeatPower>()?.Amount ?? 0;
		decimal reduceBy = System.Math.Min((decimal)current, amount);
		if (reduceBy > 0m)
		{
			await PowerCmd.Apply<HeatPower>(choiceContext, creature, -reduceBy, creature, source);
		}
	}
}
