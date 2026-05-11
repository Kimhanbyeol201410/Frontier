using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Cards;
using Frontier.Characters;

// 단조 (1코 / 공격)
// - 피해 5 (+ 열기 10당 피해 DamagePerHeat), 열기 +10, 손패에서 강화 가능한 카드 PickCount장을 선택해 이번 전투 동안 강화
// 업그레이드: 열기 10당 피해 2, 선택 강화 2장
[Pool(typeof(ShumitCardPool))]
public sealed class ForgingCard : ShumitCard
{
    private const string HeatGainKey = "HeatGain";
    private const string HeatPerDamageKey = "HeatPerDamage";
    private const string DamagePerHeatKey = "DamagePerHeat";
    private const string PickCountKey = "PickCount";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar(HeatGainKey, 10m),
        new DynamicVar(HeatPerDamageKey, 10m),
        new DynamicVar(DamagePerHeatKey, 1m),
        new DynamicVar(PickCountKey, 1m),
    };

    public ForgingCard()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        int heat = base.Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        int per = System.Math.Max(1, base.DynamicVars[HeatPerDamageKey].IntValue);
        decimal perHeatDamage = base.DynamicVars[DamagePerHeatKey].BaseValue;
        decimal bonusDamage = (heat / per) * perHeatDamage;
        decimal totalDamage = base.DynamicVars.Damage.BaseValue + bonusDamage;

        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await PowerCmd.Apply<HeatPower>(base.Owner.Creature, base.DynamicVars[HeatGainKey].BaseValue, base.Owner.Creature, this);

        int pickCount = base.DynamicVars[PickCountKey].IntValue;
        if (pickCount <= 0)
        {
            return;
        }

        if (pickCount == 1)
        {
            CardModel? selected = await CardSelectCmd.FromHandForUpgrade(choiceContext, Owner, this);
            if (selected != null && !ReferenceEquals(selected, this))
            {
                CardCmd.Upgrade(selected, CardPreviewStyle.HorizontalLayout);
            }
            return;
        }

        CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, pickCount);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            (CardModel c) => c.IsUpgradable && !ReferenceEquals(c, this),
            this);
        foreach (CardModel c in picked.ToList())
        {
            if (c.IsUpgradable)
            {
                CardCmd.Upgrade(c, CardPreviewStyle.HorizontalLayout);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[DamagePerHeatKey].UpgradeValueBy(1m);
        base.DynamicVars[PickCountKey].UpgradeValueBy(1m);
    }
}



