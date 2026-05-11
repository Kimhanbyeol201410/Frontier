using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Frontier.Characters;

namespace Frontier.Cards;

// 집게질 — 선택한 카드 1장을 «Times»회 강화. 강화 시 1회 → 2회.
[Pool(typeof(ShumitCardPool))]
public sealed class TongsCard : ShumitCard
{
    private const string HeatToCardKey = "HeatCharge";
    private const string TimesKey = "Times";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(TimesKey, 1m),
        new DynamicVar(HeatToCardKey, 5m),
    };

    public TongsCard()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // FromHand + SimpleSelect는 강화 미리보기가 없고, IsUpgradable이 아닌 카드도 골라질 수 있다. Armaments·제련소와 동일하게 FromHandForUpgrade 사용.
        CardModel? target = await CardSelectCmd.FromHandForUpgrade(choiceContext, Owner, this);
        if (target != null && !ReferenceEquals(target, this))
        {
            int times = DynamicVars[TimesKey].IntValue;
            for (int i = 0; i < times && target.IsUpgradable; i++)
            {
                CardCmd.Upgrade(target, CardPreviewStyle.HorizontalLayout);
            }
        }

        await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, DynamicVars[HeatToCardKey].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[TimesKey].UpgradeValueBy(1m);
        DynamicVars[HeatToCardKey].UpgradeValueBy(5m);
    }
}
