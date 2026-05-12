using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.CardSelection;
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
        // 0~1 자유 선택 — 강화 없이 종료(스킵) 허용. 발열은 카드 선택 결과와 무관하게 항상 적용.
        CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, 0, 1);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            (CardModel c) => c.IsUpgradable && !ReferenceEquals(c, this),
            this);
        CardModel? target = picked.FirstOrDefault();
        if (target != null)
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
