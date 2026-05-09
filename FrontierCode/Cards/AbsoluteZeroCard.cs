using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 절대영도
[Pool(typeof(ShumitCardPool))]
public sealed class AbsoluteZeroCard : ShumitCard
{
    private const string BlockPerTenKey = "BlockPerTenHeat";

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(BlockPerTenKey, 5m) };

    public AbsoluteZeroCard()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heatBefore = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        decimal bodyBurn = Owner.Creature.GetPower<BodyBurnPower>()?.Amount ?? 0m;

        await FrontierHeatUtil.ReduceHeat(Owner.Creature, heatBefore, this);
        if (bodyBurn > 0m)
        {
            await PowerCmd.Apply<BodyBurnPower>(Owner.Creature, -bodyBurn, Owner.Creature, this);
        }

        decimal perBlock = DynamicVars[BlockPerTenKey].BaseValue;
        decimal block = (heatBefore / 10) * perBlock;
        if (block > 0m)
        {
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);
        }

        CombatState cs = Owner.Creature.CombatState
            ?? throw new System.InvalidOperationException("AbsoluteZeroCard requires CombatState.");
        CardModel cold = cs.CreateCard<ColdBurnStatusCard>(Owner);
        await CardPileCmd.Add(cold, PileType.Discard);
        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BlockPerTenKey].UpgradeValueBy(3m);
    }
}
