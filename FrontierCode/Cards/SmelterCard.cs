using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class SmelterCard : ShumitCard
{
    private const string HeatPerTurnKey = "HeatPerTurn";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[]
    {
        FrontierKeywords.PreserveTrigger,
        CardKeyword.Retain,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(HeatPerTurnKey, 20m),
    };

    public SmelterCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyEffect(choiceContext);
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

        await ApplyEffect(choiceContext);
    }

    private async Task ApplyEffect(PlayerChoiceContext choiceContext)
    {
        // 0~1 자유 선택 — 강화 없이 종료(스킵) 허용. FromHand 로 호출해 MinSelect=0 적용.
        CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, 0, 1);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            (CardModel c) => c.IsUpgradable && !ReferenceEquals(c, this),
            this);
        CardModel? selected = picked.FirstOrDefault();
        if (selected != null)
        {
            CardCmd.Upgrade(selected, CardPreviewStyle.HorizontalLayout);
        }

        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, DynamicVars[HeatPerTurnKey].BaseValue, this);
    }
}
