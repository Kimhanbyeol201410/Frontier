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

// 틈새 (1코 / 일반 공격): 피해 5, 약화 1 → 강화 시 피해 7, 약화 2.
[Pool(typeof(ShumitCardPool))]
public sealed class CreviceCard : ShumitCard
{
	private const string WeakKey = "Weak";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(5m, ValueProp.Move),
		new DynamicVar(WeakKey, 1m),
	};

	public CreviceCard()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		System.ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
		await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars[WeakKey].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
		DynamicVars[WeakKey].UpgradeValueBy(1m);
	}
}
