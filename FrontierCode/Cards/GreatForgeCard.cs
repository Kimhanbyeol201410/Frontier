using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Frontier.Utilities;
using Frontier.Characters;

namespace Frontier.Cards;

// 위대한 대장간 (0코 토큰): 보존, 턴 시작 시 손에 있으면 손패 카드를 여러 번 강화.
[Pool(typeof(ShumitCardPool))]
public sealed class GreatForgeCard : TokenCardBase
{
    private const string UpgradesPerTurnKey = "UpgradesPerTurn";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Retain };

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(UpgradesPerTurnKey, 2m) };

    public GreatForgeCard()
        : base(0, CardType.Skill, TargetType.None)
    {
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this))
        {
            return;
        }

        int upgradeCount = DynamicVars[UpgradesPerTurnKey].IntValue;
        for (int i = 0; i < upgradeCount; i++)
        {
            if (!FrontierHandForgeUpgrade.TryUpgradeOneRandomFromHand(Owner))
            {
                break;
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[UpgradesPerTurnKey].UpgradeValueBy(2m);
    }
}
