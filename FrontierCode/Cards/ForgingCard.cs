using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Frontier.Cards;
using Frontier.Characters;

[Pool(typeof(ShumitCardPool))]
public sealed class ForgingCard : ShumitCard
{
    private const string PickCountKey = "PickCount";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar(PickCountKey, 1m),
    };

    public ForgingCard()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        System.ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        int pickCount = base.DynamicVars[PickCountKey].IntValue;
        if (pickCount <= 0)
        {
            return;
        }

        // 0 ~ pickCount 자유 선택 — 강화 없이 종료(스킵) 허용.
        CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, 0, pickCount);
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            (CardModel c) => c.IsUpgradable && !ReferenceEquals(c, this),
            this);
        foreach (CardModel c in picked.ToList())
        {
            if (c.IsUpgradable)
            {
                CardCmd.Upgrade(c, CardPreviewStyle.HorizontalLayout);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
        base.DynamicVars[PickCountKey].UpgradeValueBy(1m);
    }
}
