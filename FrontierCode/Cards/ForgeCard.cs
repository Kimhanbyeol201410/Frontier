using System.Collections.Generic;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Cards;
using Frontier.Characters;

// 대장간 토큰: 보존. 드로우 감소·턴당 강화는 BrokenForgeRelic 이 처리. OnPlay 불필요.
[Pool(typeof(ShumitCardPool))]
public sealed class ForgeCard : ShumitCard
{
    private const string UpgradesPerTurnKey = "UpgradesPerTurn";

    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Retain };
    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(UpgradesPerTurnKey, 1m) };

    public ForgeCard()
        : base(1, CardType.Skill, CardRarity.Event, TargetType.None, showInCardLibrary: false)
    {
    }

    protected override void OnUpgrade()
    {
        DynamicVars[UpgradesPerTurnKey].UpgradeValueBy(1m);
    }
}
