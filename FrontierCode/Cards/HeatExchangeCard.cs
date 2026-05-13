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

// 열 교환 — 손에서 카드 1장을 «UpgradeTimes»회 강화한 뒤 뽑을 더미 맨 위로 옮긴다. 강화 시 1회 → 2회 (집게질과 동일 패턴).
//
//   주의: 강화·이동 순서가 매우 중요하다. FrontierUpgradeSelectUtil 가 NPlayerHand.Mode.UpgradeSelect 로 카드를 받기 때문에
//   선택 직후 카드는 강화 「미리보기」 상태이고, 이 상태로 CardPileCmd.Add 를 호출하면 unpack/CardPileCmd.cs:285 의
//   "A card preview cannot be added to a pile." 가드에 걸려 InvalidOperationException 이 발생한다.
//   CardCmd.Upgrade -> FinalizeUpgradeInternal 가 미리보기 상태를 정리하므로, 강화를 먼저 끝낸 뒤 이동해야 안전하다.
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
            // 1) 미리보기 상태를 정리하기 위해 강화를 먼저 수행 (FinalizeUpgradeInternal 호출됨).
            int times = DynamicVars[UpgradeTimesKey].IntValue;
            for (int i = 0; i < times && moveCard.IsUpgradable; i++)
            {
                CardCmd.Upgrade(moveCard, CardPreviewStyle.HorizontalLayout);
            }

            // 2) 강화 완료 후 안전하게 뽑을 더미 맨 위로 이동.
            await CardPileCmd.Add(moveCard, PileType.Draw, CardPilePosition.Top, this);
        }

        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, DynamicVars[HeatLossKey].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[UpgradeTimesKey].UpgradeValueBy(1m);
    }
}
