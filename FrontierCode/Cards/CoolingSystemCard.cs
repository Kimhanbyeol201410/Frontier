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
public sealed class CoolingSystemCard : ShumitCard
{
    private const string HeatLossKey = "HeatLoss";
    private const string BlockPerTurnKey = "BlockPerTurn";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(HeatLossKey, 5m),
        new DynamicVar(BlockPerTurnKey, 5m),
    };

    public CoolingSystemCard()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ShumitCoolingSystemPower>(
            Owner.Creature,
            DynamicVars[HeatLossKey].BaseValue,
            Owner.Creature,
            this);

        // 카드 강화 레벨에 따른 매 턴 방어도 값을 power 에 동기화.
        ShumitCoolingSystemPower? power = Owner.Creature.GetPower<ShumitCoolingSystemPower>();
        if (power != null)
        {
            power.SetBlockPerTurn(DynamicVars[BlockPerTurnKey].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatLossKey].UpgradeValueBy(5m);
        DynamicVars[BlockPerTurnKey].UpgradeValueBy(2m);
    }
}
