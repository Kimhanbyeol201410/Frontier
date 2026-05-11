using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Frontier.Characters;

namespace Frontier.Cards;

// 대장간의 도면 — 사용 즉시 대장간/용광로 중 하나를 선택해 손패에 추가.
[Pool(typeof(ShumitCardPool))]
public sealed class ForgeBlueprintCard : ShumitCard
{
	public ForgeBlueprintCard()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await FrontierForgeBlueprintRewards.OfferMainPairAsync(Owner, choiceContext);
	}
}
