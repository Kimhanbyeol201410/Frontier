using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Cards;

// 냉각 (고급 스킬): 열화(강화 1단계 제거) 후 방어도·열기 감소.
[Pool(typeof(ShumitCardPool))]
public sealed class ThermalCoolingCard : ShumitCard
{
    private const string HeatLossKey = "HeatLoss";

    public override bool GainsBlock => true;

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { FrontierKeywords.ThermalDegradation };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(9m, ValueProp.Move),
        new DynamicVar(HeatLossKey, 20m),
    };

    public ThermalCoolingCard()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            (CardModel c) => !ReferenceEquals(c, this) && c.CurrentUpgradeLevel > 0,
            this);
        CardModel? target = picked.FirstOrDefault();
        if (target != null)
        {
            await FrontierThermalDegradationUtil.TryApplyOneStep(choiceContext, target);
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, DynamicVars[HeatLossKey].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars[HeatLossKey].UpgradeValueBy(10m);
    }
}
