using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Frontier.Characters;

namespace Frontier.Cards;

// 대장간의 도면 — 언제든 플레이, 누적 강화 5회 시(또는 이미 5회 이상이면 즉시) 대장간/용광로 선택.
[Pool(typeof(ShumitCardPool))]
public sealed class ForgeBlueprintCard : ShumitCard
{
	public ForgeBlueprintCard()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (FrontierSession.GetUpgradesThisCombat(Owner) >= 5)
		{
			await FrontierForgeBlueprintRewards.OfferMainPairAsync(Owner, choiceContext);
		}
		else
		{
			FrontierSession.QueueForgeMainBlueprint(Owner);
		}
	}
}
