using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Frontier.Characters;

namespace Frontier.Cards;

// 재사용의 미학: 이번 턴에 소멸한 화상 수만큼 드로우 (1→0코).
[Pool(typeof(ShumitCardPool))]
public sealed class ReuseAestheticsCard : ShumitCard
{
    public ReuseAestheticsCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int n = FrontierSession.BurnsExhaustedThisPlayerTurn;
        if (n > 0)
        {
            await CardPileCmd.Draw(choiceContext, n, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
