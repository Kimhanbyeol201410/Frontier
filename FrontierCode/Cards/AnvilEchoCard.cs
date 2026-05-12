using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;
using Frontier.Utilities;

namespace Frontier.Cards;

// 모루의 잔향
[Pool(typeof(ShumitCardPool))]
public sealed class AnvilEchoCard : ShumitCard
{
    private const string HitsKey = "Hits";
    private const string MasterpieceLeftKey = "MasterpieceLeft";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { FrontierKeywords.Masterpiece };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(2m, ValueProp.Move),
        new DynamicVar(HitsKey, 5m),
        new DynamicVar(MasterpieceLeftKey, 5m),
    };

    public AnvilEchoCard()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CombatState combatState = FrontierCombatStateHelper.RequireFor(Owner);
        int hits = DynamicVars[HitsKey].IntValue;
        for (int i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(combatState).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HitsKey].UpgradeValueBy(1m);
        DynamicVars[MasterpieceLeftKey].UpgradeValueBy(-1m);
    }
}
