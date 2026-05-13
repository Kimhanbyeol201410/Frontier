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

// 증기 배출 — 재련 10: 강화할수록 설명의 남은 재련 수가 10→0으로 감소.
[Pool(typeof(ShumitCardPool))]
public sealed class SteamReleaseCard : ShumitCard
{
    private const string VentDamageKey = "VentDamage";
    private const string ReforgeLeftKey = "ReforgeLeft";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(VentDamageKey, 4m),
        new DynamicVar(ReforgeLeftKey, 10m),
    };

    public SteamReleaseCard()
        : base(3, CardType.Power, CardRarity.Uncommon, TargetType.Self)
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
        DynamicVars[VentDamageKey].UpgradeValueBy(3m);
        DynamicVars[ReforgeLeftKey].UpgradeValueBy(-1m);
    }
}
