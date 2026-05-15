using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Frontier.Relics;

[Pool(typeof(EventRelicPool))]
public sealed class BrokenForgeRelic : FrontierRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;
}
