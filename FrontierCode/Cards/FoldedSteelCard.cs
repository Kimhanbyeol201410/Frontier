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

// 슈미트.md — 접쇠 (1코 / 스킬)
// 열기 70 이상일 때만 사용 가능.
// 이번 턴 플레이하는 강화된 카드 1장(업그레이드 시 2장)의 효과가 한 번 더 이어집니다.
[Pool(typeof(ShumitCardPool))]
public sealed class FoldedSteelCard : ShumitCard
{
    private const string HeatReqKey = "HeatReq";
    private const string ReplaysKey = "Replays";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(HeatReqKey, 70m),
        new DynamicVar(ReplaysKey, 1m),
    };

    protected override bool IsPlayable =>
        base.IsPlayable && HeatAtLeast(DynamicVars[HeatReqKey].IntValue);

    public FoldedSteelCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal n = DynamicVars[ReplaysKey].BaseValue;
        await PowerCmd.Apply<FoldedSteelReplayPower>(choiceContext, Owner.Creature, n, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[ReplaysKey].UpgradeValueBy(1m);
    }

    private bool HeatAtLeast(int min)
    {
        return (Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0) >= min;
    }
}
