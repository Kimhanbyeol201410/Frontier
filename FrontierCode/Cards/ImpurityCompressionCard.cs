using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 불순물 압착
[Pool(typeof(ShumitCardPool))]
public sealed class ImpurityCompressionCard : ShumitCard
{
    private const string HeatKey = "HeatGain";
    private const string PerBurnKey = "PerBurn";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move),
        new DynamicVar(HeatKey, 10m),
        new DynamicVar(PerBurnKey, 2m),
    };

    public ImpurityCompressionCard()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> burns = PileType.Draw.GetPile(Owner).Cards.Where(static (CardModel c) => c is Burn).ToList();
        int n = burns.Count;
        foreach (CardModel b in burns)
        {
            await CardCmd.Exhaust(choiceContext, b);
        }

        decimal bonus = n * DynamicVars[PerBurnKey].BaseValue;
        decimal total = DynamicVars.Damage.BaseValue + bonus;
        await DamageCmd.Attack(total).FromCard(this).TargetingAllOpponents(Owner.Creature.CombatState!).Execute(choiceContext);
        await PowerCmd.Apply<HeatPower>(Owner.Creature, DynamicVars[HeatKey].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars[PerBurnKey].UpgradeValueBy(2m);
    }
}
