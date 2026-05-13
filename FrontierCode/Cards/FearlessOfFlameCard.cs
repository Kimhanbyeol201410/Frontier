using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Frontier.Cards;

// 뜨거운 노력: 사용 시 힘을 1회 즉시 부여 + 매 턴 시작 시 열기 획득.
[Pool(typeof(ShumitCardPool))]
public sealed class FearlessOfFlameCard : ShumitCard
{
    private const string HeatPerTurnKey = "HeatPerTurn";
    private const string StrGainKey = "StrGain";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(HeatPerTurnKey, 10m),
        new DynamicVar(StrGainKey, 2m),
    };

    public FearlessOfFlameCard()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        decimal str = DynamicVars[StrGainKey].BaseValue;
        if (str > 0m)
        {
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, str, Owner.Creature, this, silent: false);
        }

        await PowerCmd.Apply<ShumitFearlessFlamePower>(
            Owner.Creature,
            DynamicVars[HeatPerTurnKey].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatPerTurnKey].UpgradeValueBy(10m);
        DynamicVars[StrGainKey].UpgradeValueBy(1m);
    }
}
