using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Frontier.Characters;
using Frontier.Powers;

namespace Frontier.Cards;

// 화염의 심장: 화상/신체 화상 면역 의도 버프(실제 면역은 후속).
[Pool(typeof(ShumitCardPool))]
public sealed class HeartOfFlameCard : ShumitCard
{
    protected override IEnumerable<CardKeyword> ShumitCanonicalKeywords => new[] { FrontierKeywords.BodyBurn };

    public HeartOfFlameCard()
        : base(3, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ShumitHeartOfFlameImmunityPower>(Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
