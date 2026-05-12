using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Frontier.Characters;
using Frontier.Powers;

namespace Frontier.Cards;

// 접쇠 (1코 / 스킬)
//   - 사용 조건 없음. 다음 번 사용하는 강화 카드 1장을 «Replays» 번 재사용.
//   - 열기가 «HeatReq»(70, 고정) 이상이면 «HeatBonus» 번 더 재사용.
//   - 강화 시: «HeatBonus» 1→2 (열기 70 이상일 때 추가 재사용 1번 → 2번). HeatReq·Replays 는 그대로.
[Pool(typeof(ShumitCardPool))]
public sealed class FoldedSteelCard : ShumitCard
{
    private const string HeatReqKey = "HeatReq";
    private const string ReplaysKey = "Replays";
    private const string HeatBonusKey = "HeatBonus";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(HeatReqKey, 70m),
        new DynamicVar(ReplaysKey, 1m),
        new DynamicVar(HeatBonusKey, 1m),
    };

    public FoldedSteelCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal replays = DynamicVars[ReplaysKey].BaseValue;
        int heatReq = DynamicVars[HeatReqKey].IntValue;
        if (HeatAtLeast(heatReq))
        {
            replays += DynamicVars[HeatBonusKey].BaseValue;
        }

        await PowerCmd.Apply<FoldedSteelReplayPower>(Owner.Creature, replays, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatBonusKey].UpgradeValueBy(1m);
    }

    private bool HeatAtLeast(int min)
    {
        return (Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0) >= min;
    }
}
