using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Powers;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Cards;

// 목숨을 걸어
[Pool(typeof(ShumitCardPool))]
public sealed class BetYourLifeCard : ShumitCard
{
    private const string StrBonusKey = "StrBonus";
    private const string DexBonusKey = "DexBonus";
    private const string BodyBurnGainKey = "BodyBurnGain";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(StrBonusKey, 1m),
        new DynamicVar(DexBonusKey, 1m),
        new DynamicVar(BodyBurnGainKey, 4m),
    };

    public BetYourLifeCard()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        FrontierSession.SetBetYourLifePerPlayBonuses(
            Owner,
            DynamicVars[StrBonusKey].IntValue,
            DynamicVars[DexBonusKey].IntValue);

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ShumitBetYourLifePower>(Owner.Creature, 1m, Owner.Creature, this);

        decimal bodyBurnGain = DynamicVars[BodyBurnGainKey].BaseValue;
        if (bodyBurnGain > 0m)
        {
            await PowerCmd.Apply<BodyBurnPower>(Owner.Creature, bodyBurnGain, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
