using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

[Pool(typeof(ShumitCardPool))]
public sealed class HeatedShieldCard : ShumitCard
{
    private const string BlockPerHeatChunkKey = "BlockPerHeatChunk";
    private const int HeatChunk = 10;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(8m, ValueProp.Move),
        new DynamicVar(BlockPerHeatChunkKey, 1m),
    };

    public HeatedShieldCard()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        decimal bonus = (heat / HeatChunk) * DynamicVars[BlockPerHeatChunkKey].BaseValue;
        decimal total = DynamicVars.Block.BaseValue + bonus;
        await CreatureCmd.GainBlock(Owner.Creature, total, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BlockPerHeatChunkKey].UpgradeValueBy(1m);
    }
}
