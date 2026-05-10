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
public sealed class SteamReleaseCard : ShumitCard
{
    private const string VentDamageKey = "VentDamage";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(VentDamageKey, 5m) };

    public SteamReleaseCard()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ShumitSteamVentPower>(
            Owner.Creature,
            DynamicVars[VentDamageKey].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[VentDamageKey].UpgradeValueBy(5m);
    }
}
