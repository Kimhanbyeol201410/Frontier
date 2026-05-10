using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Frontier.Characters;

namespace Frontier.Cards;

// 제련 설계: 드로우 + 다음 공격에 열기 (1→0코).
[Pool(typeof(ShumitCardPool))]
public sealed class SmeltingDesignCard : ShumitCard
{
    private const string NextHeatKey = "NextAttackHeat";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(NextHeatKey, 5m) };

    public SmeltingDesignCard()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 1, Owner);
        await PowerCmd.Apply<ShumitNextAttackHeatPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars[NextHeatKey].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
