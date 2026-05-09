using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Frontier.Characters;

namespace Frontier.Cards;

// 금속 액화 (2→1코)
[Pool(typeof(ShumitCardPool))]
public sealed class MetalLiquefactionCard : ShumitCard
{
    private const string BonusHeatKey = "BonusHeat";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(BonusHeatKey, 10m) };

    public MetalLiquefactionCard()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, 1);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            (CardModel c) => c.Type == CardType.Attack && c.CurrentUpgradeLevel > 0 && !ReferenceEquals(c, this),
            this);
        CardModel? target = picked.FirstOrDefault();
        if (target != null)
        {
            target.EnergyCost.SetThisTurnOrUntilPlayed(0);
        }

        await PowerCmd.Apply<ShumitUpgradedAttackBonusHeatPower>(
            Owner.Creature,
            DynamicVars[BonusHeatKey].BaseValue,
            Owner.Creature,
            this);

        CombatState combatState = Owner.Creature.CombatState
            ?? throw new System.InvalidOperationException("MetalLiquefactionCard requires CombatState.");
        for (int i = 0; i < 2; i++)
        {
            CardModel burn = combatState.CreateCard<Burn>(Owner);
            await CardPileCmd.Add(burn, PileType.Draw, CardPilePosition.Random, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
