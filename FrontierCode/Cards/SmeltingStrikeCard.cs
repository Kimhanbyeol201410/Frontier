using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;
using Frontier.Utilities;

namespace Frontier.Cards;

// 제련 타격
[Pool(typeof(ShumitCardPool))]
public sealed class SmeltingStrikeCard : ShumitCard
{
    private const string HeatGainKey = "HeatGain";
    private const string UpgradesKey = "HandUpgrades";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar(HeatGainKey, 10m),
        new DynamicVar(UpgradesKey, 1m),
    };

    public SmeltingStrikeCard()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        int n = DynamicVars[UpgradesKey].IntValue;
        if (FrontierCombatStateHelper.TryGetFor(Owner) is not CombatState combatState)
        {
            throw new System.InvalidOperationException("SmeltingStrikeCard requires CombatState.");
        }
        for (int i = 0; i < n; i++)
        {
            var list = PileType.Hand.GetPile(Owner).Cards.Where((CardModel c) => !ReferenceEquals(c, this) && c.IsUpgradable).ToList();
            if (list.Count == 0)
            {
                break;
            }

            int idx = combatState.RunState.Rng.Shuffle.NextInt(list.Count);
            CardCmd.Upgrade(list[idx], CardPreviewStyle.HorizontalLayout);
        }

        await PowerCmd.Apply<HeatPower>(Owner.Creature, DynamicVars[HeatGainKey].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars[UpgradesKey].UpgradeValueBy(1m);
    }
}
