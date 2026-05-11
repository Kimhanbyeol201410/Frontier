using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Frontier.Characters;

namespace Frontier.Cards;

// 대장간 시설의 도면 — 언제든 플레이, 누적 강화 5회 시(또는 이미 5회 이상이면 즉시) 연마실/제련소 선택.
[Pool(typeof(ShumitCardPool))]
public sealed class ForgeFacilityBlueprintCard : ShumitCard
{
	public ForgeFacilityBlueprintCard()
		: base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (FrontierSession.GetUpgradesThisCombat(Owner) >= 5)
		{
			await FrontierForgeBlueprintRewards.OfferFacilityPairAsync(Owner, choiceContext);
		}
		else
		{
			FrontierSession.QueueForgeFacilityBlueprint(Owner);
		}
	}
}
