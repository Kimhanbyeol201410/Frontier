using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Frontier.Characters;

namespace Frontier.Cards;

// 대장장이의 가호
[Pool(typeof(ShumitCardPool))]
public sealed class BlacksmithsBlessingCard : ShumitCard
{
    private const string HeatPerStatKey = "HeatPerStat";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar(HeatPerStatKey, 40m),
    };

    public BlacksmithsBlessingCard()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        int per = System.Math.Max(1, DynamicVars[HeatPerStatKey].IntValue);
        int stacks = heat / per;
        if (stacks > 0)
        {
            decimal amt = stacks;
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, amt, Owner.Creature, this);
            await PowerCmd.Apply<DexterityPower>(Owner.Creature, amt, Owner.Creature, this);
        }

        if (heat > 0)
        {
            await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, heat, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatPerStatKey].UpgradeValueBy(-15m);
    }
}
