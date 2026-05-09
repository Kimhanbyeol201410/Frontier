using System.Collections.Generic;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Frontier.Characters;

namespace Frontier.Cards;

// PDF ??(???.pdf) ?? ?? ??
// ???? ?? ?
// - ?? ??? ???.pdf ??? ???? ?? ??
[Pool(typeof(ShumitCardPool))]
public sealed class UnburningBodyCard : ShumitCard
{
public UnburningBodyCard() : base(2, CardType.Skill, CardRarity.Common, TargetType.None) { }
}



