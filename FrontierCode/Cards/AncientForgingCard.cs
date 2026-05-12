using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

/// <summary>
/// 고대의 단조 — <c>ArchaicTooth</c>(고대의 이빨) 유물이
/// 시작 카드 <c>ForgingCard</c>(단조) 를 변환해 얻게 되는 슈미트 전용 Ancient 카드.
/// 0 코스트, 단일 공격 + 손패의 모든 카드 강화.
/// </summary>
[Pool(typeof(ShumitCardPool))]
public sealed class AncientForgingCard : ShumitCard
{
    public AncientForgingCard()
        : base(0, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move),
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        IReadOnlyList<CardModel> hand = PileType.Hand.GetPile(Owner).Cards;
        foreach (CardModel c in hand.ToList())
        {
            if (ReferenceEquals(c, this))
            {
                continue;
            }
            if (c.IsUpgradable)
            {
                CardCmd.Upgrade(c, CardPreviewStyle.HorizontalLayout);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
