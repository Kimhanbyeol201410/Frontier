using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 열 교환 — 손에서 카드 1장을 뽑을 더미 맨 위로 옮긴 뒤 «UpgradeTimes»회 강화. 강화 시 1회 → 2회 (집게질과 동일 패턴).
[Pool(typeof(ShumitCardPool))]
public sealed class HeatExchangeCard : ShumitCard
{
    private const string HeatLossKey = "HeatLoss";
    private const string UpgradeTimesKey = "UpgradeTimes";

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(10m, ValueProp.Move),
        new DynamicVar(UpgradeTimesKey, 1m),
        new DynamicVar(HeatLossKey, 10m),
    };

    public HeatExchangeCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        // 0~1 자유 선택 + 강화 미리보기 — 강화·이동 없이 종료(스킵) 허용. 방어도/열기 감소는 카드 선택과 무관하게 항상 적용.
        IReadOnlyList<CardModel> picked = await FrontierUpgradeSelectUtil.SelectFromHandWithPreviewAsync(Owner, this, 1);
        CardModel? moveCard = picked.FirstOrDefault();
        if (moveCard != null)
        {
            await CardPileCmd.Add(moveCard, PileType.Draw, CardPilePosition.Top, this);

            int times = DynamicVars[UpgradeTimesKey].IntValue;
            for (int i = 0; i < times && moveCard.IsUpgradable; i++)
            {
                CardCmd.Upgrade(moveCard, CardPreviewStyle.HorizontalLayout);
            }
        }

        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, DynamicVars[HeatLossKey].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[UpgradeTimesKey].UpgradeValueBy(1m);
    }
}
