using System.Collections.Generic;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Frontier.Cards;

[Pool(typeof(EventCardPool))]
public sealed class ForgeCard : ShumitCard
{
    private const string UpgradesPerTurnKey = "UpgradesPerTurn";
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar(UpgradesPerTurnKey, 1m) };
    public override List<(string, string)> Localization => new()
    {
        ("title", "대장간"),
        ("description", "보존. 매 턴마다 카드를 1장 적게 뽑고, 손패에 있는 카드 1장을 이번 전투 동안 {UpgradesPerTurn:diff()}번 강화합니다.")
    };

    public ForgeCard() : base(1, CardType.Skill, CardRarity.Event, TargetType.None, showInCardLibrary: false) { }

    protected override void OnUpgrade()
    {
        base.DynamicVars[UpgradesPerTurnKey].UpgradeValueBy(1m);
    }
}
