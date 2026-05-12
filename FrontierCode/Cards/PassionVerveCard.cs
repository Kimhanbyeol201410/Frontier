using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class PassionVerveCard : ShumitCard
{
	private const string VulnerableKey = "Vulnerable";
	private const int Hits = 2;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(7m, ValueProp.Move),
		new DynamicVar(VulnerableKey, 2m),
	};

	public PassionVerveCard()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		System.ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		for (int i = 0; i < Hits; i++)
		{
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this)
				.Targeting(cardPlay.Target)
				.Execute(choiceContext);
		}
		await PowerCmd.Apply<VulnerablePower>(
			cardPlay.Target,
			DynamicVars[VulnerableKey].BaseValue,
			Owner.Creature,
			this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
		DynamicVars[VulnerableKey].UpgradeValueBy(1m);
	}
}
