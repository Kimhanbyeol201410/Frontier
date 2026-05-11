using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Frontier.Characters;

namespace Frontier.Cards;

// 대장간 시설의 도면 — 사용 즉시 연마실/제련소 중 하나를 선택해 손패에 추가.
[Pool(typeof(ShumitCardPool))]
public sealed class ForgeFacilityBlueprintCard : ShumitCard
{
	public ForgeFacilityBlueprintCard()
		: base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await FrontierForgeBlueprintRewards.OfferFacilityPairAsync(Owner, choiceContext);
	}
}
