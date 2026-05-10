using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Frontier.Characters;

namespace Frontier.Cards;

// 화력발전기
[Pool(typeof(ShumitCardPool))]
public sealed class FirePowerPlantCard : ShumitCard
{
    private const string EnergyOnBurnKey = "EnergyOnBurn";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new EnergyVar(EnergyOnBurnKey, 1) };

    public FirePowerPlantCard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override void OnUpgrade()
    {
        DynamicVars[EnergyOnBurnKey].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            Owner,
            prefs,
            (CardModel c) => !ReferenceEquals(c, this),
            this);
        CardModel? victim = picked.FirstOrDefault();
        if (victim != null)
        {
            bool isBurn = victim.GetType() == typeof(Burn);
            await CardCmd.Exhaust(choiceContext, victim);
            int energy = DynamicVars[EnergyOnBurnKey].IntValue;
            if (isBurn)
            {
                await PlayerCmd.GainEnergy(energy, Owner);
            }
        }
    }
}
