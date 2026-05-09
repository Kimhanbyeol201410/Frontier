using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 단층 파괴
[Pool(typeof(ShumitCardPool))]
public sealed class FaultBreakCard : ShumitCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DamageVar(26m, ValueProp.Move) };

    public FaultBreakCard()
        : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CombatState cs = Owner.Creature.CombatState!;
        foreach (Creature e in cs.HittableEnemies.ToList())
        {
            decimal hadBlock = e.Block;
            decimal dmg = hadBlock > 0m ? DynamicVars.Damage.BaseValue * 2m : DynamicVars.Damage.BaseValue;
            await DamageCmd.Attack(dmg).FromCard(this).Targeting(e).Execute(choiceContext);
            if (e.Block > 0m)
            {
                await CreatureCmd.LoseBlock(e, e.Block);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
    }
}
