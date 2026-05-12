using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class ForgeCard : ShumitCard
{
    private const string UpgradesPerTurnKey = "UpgradesPerTurn";
    private const string HeatPerTurnKey = "HeatPerTurn";

    /// <summary>「보존 발동」이 retain 효과를 포함하므로 vanilla 「보존」 키워드는 생략. <see cref="FrontierPreserveTriggerRetainPatch"/> 가 retain 동작을 보강한다.</summary>
    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[]
    {
        FrontierKeywords.PreserveTrigger,
        FrontierKeywords.Unupgradable,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(UpgradesPerTurnKey, 2m),
        new DynamicVar(HeatPerTurnKey, 10m),
    };

    /// <summary>«강화 불가» — 재련/걸작과 무관하게 강화 불가능. <see cref="FrontierUpgradeCapPatch"/> 가 0 이하 값을 그대로 유지한다.</summary>
    public override int MaxUpgradeLevel => 0;

    public ForgeCard()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyEffect(choiceContext);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this))
        {
            return;
        }

        await ApplyEffect(choiceContext);
    }

    private async Task ApplyEffect(PlayerChoiceContext choiceContext)
    {
        int times = DynamicVars[UpgradesPerTurnKey].IntValue;
        for (int i = 0; i < times; i++)
        {
            if (!FrontierHandForgeUpgrade.TryUpgradeOneRandomFromHand(Owner))
            {
                break;
            }
        }

        await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, DynamicVars[HeatPerTurnKey].BaseValue, this);
    }
}
