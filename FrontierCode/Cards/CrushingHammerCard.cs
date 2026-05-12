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

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class CrushingHammerCard : ShumitCard
{
    private const string DebuffStacksKey = "DebuffStacks";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar(DebuffStacksKey, 1m),
    };

    public CrushingHammerCard()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        decimal stacks = DynamicVars[DebuffStacksKey].BaseValue;
        await PowerCmd.Apply<WeakPower>(cardPlay.Target, stacks, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, stacks, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DebuffStacksKey].UpgradeValueBy(1m);
    }
}
