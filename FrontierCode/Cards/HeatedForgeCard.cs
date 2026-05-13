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

// 뜨거워진 대장간: 매 턴 종료 시 현재 열기를 «HeatDivisor»로 나눈 만큼 체력 회복.
//   - 강화: 분모 -10/회 (재련 2면 50→40→30, 재련 4면 50→40→30→20→10).
[Pool(typeof(ShumitCardPool))]
public sealed class HeatedForgeCard : ShumitCard
{
    private const string HeatDivisorKey = "HeatDivisor";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(HeatDivisorKey, 50m) };

    public HeatedForgeCard()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ShumitHeatedForgePower>(
            Owner.Creature,
            DynamicVars[HeatDivisorKey].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatDivisorKey].UpgradeValueBy(-10m);
    }
}
