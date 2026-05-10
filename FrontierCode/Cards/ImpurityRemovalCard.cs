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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Frontier.Characters;

namespace Frontier.Cards;

// 불순물 제거
[Pool(typeof(ShumitCardPool))]
public sealed class ImpurityRemovalCard : ShumitCard
{
    public override bool GainsBlock => true;

    private const string BurnExhaustTimesKey = "BurnExhaustTimes";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(5m, ValueProp.Move),
        new DynamicVar(BurnExhaustTimesKey, 1m),
    };

    public ImpurityRemovalCard()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        int times = DynamicVars[BurnExhaustTimesKey].IntValue;
        for (int t = 0; t < times; t++)
        {
            List<CardModel> burns = PileType.Hand.GetPile(Owner).Cards
                .Concat(PileType.Discard.GetPile(Owner).Cards)
                .Where(static (CardModel c) => c is Burn)
                .Distinct()
                .ToList();
            if (burns.Count == 0)
            {
                break;
            }

            CardSelectorPrefs prefs = new(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
            IEnumerable<CardModel> picked = await CardSelectCmd.FromSimpleGrid(choiceContext, burns, Owner, prefs);
            CardModel? b = picked.FirstOrDefault();
            if (b != null)
            {
                await CardCmd.Exhaust(choiceContext, b);
            }
            else
            {
                break;
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars[BurnExhaustTimesKey].UpgradeValueBy(1m);
    }
}
