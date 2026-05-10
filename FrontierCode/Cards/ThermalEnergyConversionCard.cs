using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 열에너지 전환: 열기를 에너지로 전환·제거, X 드로우, 소멸.
[Pool(typeof(ShumitCardPool))]
public sealed class ThermalEnergyConversionCard : ShumitCard
{
    private const string HeatPerEnergyKey = "HeatPerEnergy";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyXVar(),
        new DynamicVar(HeatPerEnergyKey, 20m),
        new EnergyVar("ThermalEnergyPer", 1),
    };

    public ThermalEnergyConversionCard()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        int per = DynamicVars[HeatPerEnergyKey].IntValue;
        if (per > 0 && heat > 0)
        {
            int energyGain = heat / per;
            if (energyGain > 0)
            {
                await PlayerCmd.GainEnergy(energyGain, Owner);
            }
        }

        await FrontierHeatUtil.ReduceHeat(choiceContext, Owner.Creature, heat, this);

        int x = ResolveEnergyXValue();
        if (x > 0)
        {
            await CardPileCmd.Draw(choiceContext, x, Owner);
        }

        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HeatPerEnergyKey].UpgradeValueBy(-10m);
    }
}
