using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Frontier.Characters;

namespace Frontier.Cards;

// 불사르지 않는 몸 — 로컬 설명만 정의, 전투 효과 미구현.
[Pool(typeof(ShumitCardPool))]
public sealed class UnburningBodyCard : ShumitCard
{
    public UnburningBodyCard()
        : base(2, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }
}



