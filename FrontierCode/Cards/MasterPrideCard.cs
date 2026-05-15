using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Characters;
using Frontier.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Frontier.Cards;

// 명인의 긍지 (3→2코 파워): 강화 시 방어도 — ShumitMasterPridePower + CardCmd.Upgrade 패치.
[Pool(typeof(ShumitCardPool))]
public sealed class MasterPrideCard : ShumitCard
{
    private const string BlockPerUpgradeKey = "BlockPerUpgrade";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(BlockPerUpgradeKey, 3m) };

    public MasterPrideCard()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ShumitMasterPridePower>(
            Owner.Creature,
            DynamicVars[BlockPerUpgradeKey].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BlockPerUpgradeKey].UpgradeValueBy(2m);
    }
}
