using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Utilities;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Cards;

/// <summary>
/// 열 교환 — 방어도 + 손패 1장을 [강화] + 열기 감소. 강화된 카드는 손에 그대로 남는다.
/// 카드 선택/강화 로직은 본 파일 안에서 자체 구현(외부 유틸 의존 X).
/// </summary>
[Pool(typeof(ShumitCardPool))]
public sealed class HeatExchangeCard : ShumitCard
{
    private const string HeatLossKey = "HeatLoss";
    private const string LogTag = "[Frontier:HeatExchange]";

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(10m, ValueProp.Move),
        new DynamicVar(HeatLossKey, 10m),
    };

    public HeatExchangeCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        GD.Print($"{LogTag} OnPlay BEGIN — upgrade={CurrentUpgradeLevel}, Block={(int)DynamicVars.Block.BaseValue}, HeatLoss={DynamicVars[HeatLossKey].IntValue}");

        // 1) 방어도.
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        // 2) 손에서 강화 가능한 카드 1장 선택 → 그 자리에서 강화.
        //    카드는 손에 남아 있고, NPlayerHand.OnSelectModeSourceFinished 가 자연스럽게 비주얼을 복원한다.
        CardModel? target = await PickUpgradableFromHandAsync();
        if (target != null)
        {
            GD.Print($"{LogTag} Picked — id={target.Id.Entry}, upgradeBefore={target.CurrentUpgradeLevel}");
            UpgradeOnce(target);
            GD.Print($"{LogTag} Upgraded — upgradeAfter={target.CurrentUpgradeLevel}, pile={target.Pile?.Type}");
        }

        // 3) 열기 감소.
        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, DynamicVars[HeatLossKey].BaseValue, this);

        GD.Print($"{LogTag} OnPlay END — currentHeat={Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0}");
    }

    protected override void OnUpgrade()
    {
        // 강화 보너스: 방어도 +4.
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    /// <summary>
    /// 손에서 강화 가능한 카드를 0~1장 선택받는다. 강화 미리보기(좌:원본 / 우:강화본) UI 사용.
    /// 강화 가능한 카드가 없거나 전투가 끝나가는 중이면 선택 화면을 띄우지 않고 null 반환(softlock 방지).
    /// </summary>
    private async Task<CardModel?> PickUpgradableFromHandAsync()
    {
        if (CombatManager.Instance.IsOverOrEnding)
        {
            GD.Print($"{LogTag} Skip selection — combat over/ending");
            return null;
        }

        NPlayerHand? hand = NCombatRoom.Instance?.Ui?.Hand;
        if (hand == null)
        {
            GD.Print($"{LogTag} Skip selection — hand UI not available");
            return null;
        }

        bool Filter(CardModel c) => c.IsUpgradable && !ReferenceEquals(c, this);

        List<CardModel> candidates = PileType.Hand.GetPile(Owner).Cards.Where(Filter).ToList();
        if (candidates.Count == 0)
        {
            GD.Print($"{LogTag} Skip selection — no upgradable cards in hand");
            return null;
        }

        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 0, 1);
        IEnumerable<CardModel> picked = await hand.SelectCards(prefs, Filter, this, NPlayerHand.Mode.UpgradeSelect);
        return picked.FirstOrDefault();
    }

    private static void UpgradeOnce(CardModel card)
    {
        if (!card.IsUpgradable)
        {
            return;
        }
        // CardCmd.Upgrade 는 내부에서 UpgradeInternal + FinalizeUpgradeInternal 을 동기로 수행한다.
        // → 이 줄을 지나면 모델의 CurrentUpgradeLevel, DynamicVars 가 즉시 갱신된 상태.
        CardCmd.Upgrade(card, CardPreviewStyle.HorizontalLayout);
    }
}
