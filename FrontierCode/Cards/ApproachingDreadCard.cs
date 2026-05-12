using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;
using Frontier.Powers;

namespace Frontier.Cards;

// 다가가는 공포
[Pool(typeof(ShumitCardPool))]
public sealed class ApproachingDreadCard : ShumitCard
{
    private const string HeatChunkKey = "HeatChunk";

    private const string StrMultKey = "StrMult";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar(HeatChunkKey, 20m),
        new DynamicVar(StrMultKey, 1m),
    };

    public ApproachingDreadCard()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        int chunk = System.Math.Max(1, DynamicVars[HeatChunkKey].IntValue);
        decimal str = (heat / chunk) * DynamicVars[StrMultKey].BaseValue;
        if (str > 0m)
        {
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, str, Owner.Creature, this);
            await PowerCmd.Apply<ShumitStripStrengthAtTurnEndPower>(Owner.Creature, str, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars[StrMultKey].UpgradeValueBy(1m);
    }
}
