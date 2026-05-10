using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 절대영도: 열기 전부 제거 → 제거한 양만큼 방어, 소멸. 강화 시 비용 -1.
[Pool(typeof(ShumitCardPool))]
public sealed class AbsoluteZeroCard : ShumitCard
{
    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override bool GainsBlock => true;

    public AbsoluteZeroCard()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heatBefore = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, heatBefore, this);
        if (heatBefore > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, heatBefore, ValueProp.Move, cardPlay);
        }

        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
