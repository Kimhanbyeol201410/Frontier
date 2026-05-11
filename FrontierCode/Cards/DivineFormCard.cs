using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Frontier.Characters;
using Frontier.Powers;

namespace Frontier.Cards;

// 신의 형상 (3코 희귀 파워, 강화 시 2코): 내 턴 시작마다 손패의 강화 가능한 모든 카드 1회 강화.
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
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
