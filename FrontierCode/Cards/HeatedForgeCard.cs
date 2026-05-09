using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Frontier.Characters;
using Frontier.Powers;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class HeatedForgeCard : ShumitCard
{
    private const string HeatKey = "HeatGain";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(HeatKey, 10m) };

    public HeatedForgeCard()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ShumitHeatedForgePower>(
            Owner.Creature,
            DynamicVars[HeatKey].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatKey].UpgradeValueBy(10m);
    }
}
