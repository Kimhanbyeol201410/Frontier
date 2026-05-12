using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Frontier.Characters;
using Frontier.Powers;

namespace Frontier.Cards;

// 신의 형상 (3코 희귀 파워, 강화 시 2코):
//   - 사용 즉시 손패의 강화 가능한 모든 카드를 1회 강화.
//   - 이후 매 내 턴 시작마다 같은 효과 반복(ShumitDivineFormPower).
[Pool(typeof(ShumitCardPool))]
public sealed class DivineFormCard : ShumitCard
{
	public DivineFormCard()
		: base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		await PowerCmd.Apply<ShumitDivineFormPower>(Owner.Creature, 1m, Owner.Creature, this);

		// 사용 시점에도 1회 즉시 강화. ShumitDivineFormPower.AfterPlayerTurnStart와 동일한 일괄 처리 패턴.
		List<CardModel> toUpgrade = PileType.Hand.GetPile(Owner).Cards
			.Where(c => c is { IsUpgradable: true } && !ReferenceEquals(c, this))
			.ToList();
		if (toUpgrade.Count > 0)
		{
			CardCmd.Upgrade(toUpgrade, CardPreviewStyle.HorizontalLayout);
		}
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
