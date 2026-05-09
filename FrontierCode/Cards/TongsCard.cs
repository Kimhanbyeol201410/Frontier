using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Frontier.Characters;

namespace Frontier.Cards;

// 집게질
[Pool(typeof(ShumitCardPool))]
public sealed class TongsCard : ShumitCard
{
    private const string HeatToCardKey = "HeatCharge";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(HeatToCardKey, 5m) };

    public TongsCard()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, 1);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            (CardModel c) => !ReferenceEquals(c, this),
            this);
        CardModel? target = picked.FirstOrDefault();
        if (target != null && target.IsUpgradable)
        {
            CardCmd.Upgrade(target, CardPreviewStyle.HorizontalLayout);
        }

        await FrontierHeatUtil.ApplyHeat(Owner.Creature, DynamicVars[HeatToCardKey].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatToCardKey].UpgradeValueBy(5m);
    }
}
