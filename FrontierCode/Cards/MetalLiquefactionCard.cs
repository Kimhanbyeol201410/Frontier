using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Frontier.Characters;

namespace Frontier.Cards;

// 금속 액화: 이번 턴 강화 공격 0코(파워) · 강화 공격 사용 시마다 열기·화상 1장(뽑을 더미) · 소멸.
[Pool(typeof(ShumitCardPool))]
public sealed class MetalLiquefactionCard : ShumitCard
{
    private const string BonusHeatKey = "BonusHeat";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(BonusHeatKey, 10m) };

    public MetalLiquefactionCard()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ShumitUpgradedAttackBonusHeatPower>(
            Owner.Creature,
            DynamicVars[BonusHeatKey].BaseValue,
            Owner.Creature,
            this);

        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
