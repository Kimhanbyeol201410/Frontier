using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Cards;
using Frontier.Characters;

// PDF 원문(슈미트.pdf) 기준 카드 정보
// 단조 (2코 / 공격)
// - 피해 8, 손패 무작위 카드 1장 이번 턴 강화, 열기 +10
// 업그레이드: 피해 11
[Pool(typeof(ShumitCardPool))]
public sealed class ForgingCard : ShumitCard
{
    private const string HeatGainKey = "HeatGain";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar(HeatGainKey, 10m),
    };

    public ForgingCard()
        : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await PowerCmd.Apply<HeatPower>(choiceContext, base.Owner.Creature, base.DynamicVars[HeatGainKey].BaseValue, base.Owner.Creature, this);

        if (base.Owner.Creature.CombatState is not CombatState combatState)
        {
            throw new System.InvalidOperationException("ForgingCard.OnPlay requires CombatState.");
        }
        var candidates = PileType.Hand.GetPile(base.Owner).Cards
            .Where((c) => c != this && c.IsUpgradable)
            .ToList();
        if (candidates.Count > 0)
        {
            int idx = combatState.RunState.Rng.Shuffle.NextInt(candidates.Count);
            CardCmd.Upgrade(candidates[idx], CardPreviewStyle.HorizontalLayout);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
    }
}



