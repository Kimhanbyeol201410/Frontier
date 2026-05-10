using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;
using Frontier.Powers;

namespace Frontier.Cards;

// 화염의 갑옷: 플레이 시 방어 + 화상 플레이 시 방어 버프.
[Pool(typeof(ShumitCardPool))]
public sealed class FlameArmorCard : ShumitCard
{
    private const string BlockOnBurnKey = "BlockOnBurn";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(4m, ValueProp.Move),
        new DynamicVar(BlockOnBurnKey, 4m),
    };

    public FlameArmorCard()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<ShumitFlameArmorPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars[BlockOnBurnKey].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars[BlockOnBurnKey].UpgradeValueBy(2m);
    }
}
