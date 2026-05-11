using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Powers;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Frontier.Cards;

// 목숨을 걸어
[Pool(typeof(ShumitCardPool))]
public sealed class BetYourLifeCard : ShumitCard
{
    private const string StrBonusKey = "StrBonus";
    private const string DexBonusKey = "DexBonus";
    private const string HeatGainKey = "HeatGain";

    private static readonly PileType[] UpgradePiles =
    {
        PileType.Hand,
        PileType.Draw,
        PileType.Discard,
        PileType.Exhaust,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(StrBonusKey, 1m),
        new DynamicVar(DexBonusKey, 1m),
        new DynamicVar(HeatGainKey, 200m),
    };

    public BetYourLifeCard()
        : base(0, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        FrontierSession.SetBetYourLifePerPlayBonuses(
            Owner,
            DynamicVars[StrBonusKey].IntValue,
            DynamicVars[DexBonusKey].IntValue);

        foreach (PileType pileType in UpgradePiles)
        {
            IReadOnlyList<CardModel> cards = pileType.GetPile(Owner).Cards;
            if (cards.Count == 0)
            {
                continue;
            }

            foreach (CardModel c in cards.ToList())
            {
                if (c.IsUpgradable)
                {
                    CardCmd.Upgrade(c, CardPreviewStyle.HorizontalLayout);
                }
            }
        }

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ShumitBetYourLifePower>(Owner.Creature, 1m, Owner.Creature, this);

        decimal heatGain = DynamicVars[HeatGainKey].BaseValue;
        if (heatGain > 0m)
        {
            await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, heatGain, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[StrBonusKey].UpgradeValueBy(1m);
        DynamicVars[DexBonusKey].UpgradeValueBy(1m);
    }
}
