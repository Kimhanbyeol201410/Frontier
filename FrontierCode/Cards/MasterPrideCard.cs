using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Frontier.Characters;

namespace Frontier.Cards;

// 명인의 긍지 (3→2코)
[Pool(typeof(ShumitCardPool))]
public sealed class MasterPrideCard : ShumitCard
{
    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { CardKeyword.Exhaust };

    public MasterPrideCard()
        : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        void UpgradePile(PileType pile)
        {
            foreach (CardModel c in pile.GetPile(Owner).Cards.ToList())
            {
                if (c.IsUpgradable && !ReferenceEquals(c, this))
                {
                    CardCmd.Upgrade(c, CardPreviewStyle.HorizontalLayout);
                }
            }
        }

        UpgradePile(PileType.Hand);
        UpgradePile(PileType.Draw);

        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        if (heat <= 50)
        {
            decimal need = 50m - heat;
            if (need > 0m)
            {
                await PowerCmd.Apply<HeatPower>(choiceContext, Owner.Creature, need, Owner.Creature, this);
            }
        }

        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
