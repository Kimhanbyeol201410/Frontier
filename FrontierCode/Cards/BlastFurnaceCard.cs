using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Frontier.Characters;

namespace Frontier.Cards;

// 용광로 (0코 토큰): 보존, 턴 시작 시 손에 있으면 열기 및 손패 소멸.
[Pool(typeof(ShumitCardPool))]
public sealed class BlastFurnaceCard : TokenCardBase
{
    private const string HeatPerTurnKey = "HeatPerTurn";
    private const string ExhaustCountKey = "ExhaustCount";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Retain };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(HeatPerTurnKey, 15m),
        new DynamicVar(ExhaustCountKey, 1m),
    };

    public BlastFurnaceCard()
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

        await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, DynamicVars[HeatPerTurnKey].BaseValue, null);

        int exhaustTimes = DynamicVars[ExhaustCountKey].IntValue;
        CardSelectorPrefs prefs = new(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        for (int i = 0; i < exhaustTimes; i++)
        {
            IEnumerable<CardModel> picked = await CardSelectCmd.FromHandForDiscard(
                choiceContext,
                Owner,
                prefs,
                (CardModel c) => c != this,
                this);
            CardModel? victim = picked.FirstOrDefault();
            if (victim == null)
            {
                break;
            }

            await CardCmd.Exhaust(choiceContext, victim);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[ExhaustCountKey].UpgradeValueBy(1m);
    }
}
