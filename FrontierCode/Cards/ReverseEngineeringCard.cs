using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Frontier.Characters;

namespace Frontier.Cards;

/// <summary>리버스 엔지니어링 — 손패 1장 소멸 후 그 카드의 표시 비용만큼 에너지, 피해 수치만큼 이번 턴 힘, 방어 수치만큼 이번 턴 민첩, 이번 턴 열기 증감 반전.</summary>
[Pool(typeof(ShumitCardPool))]
public sealed class ReverseEngineeringCard : ShumitCard
{
	public ReverseEngineeringCard()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
	{
	}

	protected override bool IsPlayable =>
		base.IsPlayable && PileType.Hand.GetPile(Owner).Cards.Any(c => !ReferenceEquals(c, this));

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardSelectorPrefs prefs = new(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
		IEnumerable<CardModel> picked = await CardSelectCmd.FromHandForDiscard(
			choiceContext,
			Owner,
			prefs,
			(CardModel c) => !ReferenceEquals(c, this),
			this);
		CardModel? victim = picked.FirstOrDefault();
		if (victim == null)
		{
			return;
		}

		int energyGain = System.Math.Max(0, victim.EnergyCost.GetAmountToSpend());
		decimal damageListed = SafeDynamicVarBase(victim, "Damage");
		decimal blockListed = SafeDynamicVarBase(victim, "Block");

		await CardCmd.Exhaust(choiceContext, victim);

		if (energyGain > 0)
		{
			await PlayerCmd.GainEnergy(energyGain, Owner);
		}

		if (damageListed > 0m)
		{
			await PowerCmd.Apply<StrengthPower>(Owner.Creature, damageListed, Owner.Creature, this);
			await PowerCmd.Apply<ShumitStripStrengthAtTurnEndPower>(Owner.Creature, damageListed, Owner.Creature, this);
		}

		if (blockListed > 0m)
		{
			await PowerCmd.Apply<DexterityPower>(Owner.Creature, blockListed, Owner.Creature, this);
			await PowerCmd.Apply<ShumitStripDexterityAtTurnEndPower>(Owner.Creature, blockListed, Owner.Creature, this);
		}

		await PowerCmd.Apply<ShumitReverseEngineeringInvertHeatPower>(Owner.Creature, 1m, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}

	private static decimal SafeDynamicVarBase(CardModel card, string key)
	{
		try
		{
			return card.DynamicVars[key].BaseValue;
		}
		catch
		{
			return 0m;
		}
	}
}
