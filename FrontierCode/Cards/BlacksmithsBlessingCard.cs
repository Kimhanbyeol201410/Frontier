using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Frontier.Utilities;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Frontier.Characters;

namespace Frontier.Cards;

// 대장장이의 가호 — «열기 HeatPerStat 마다 StatPerStack 의 힘/민첩» 패턴.
//   - 기본: 열기 40당 힘 1, 민첩 1.
//   - 강화: 열기 40당 얻는 힘/민첩 1 → 2 (StatPerStack +1). 발동 빈도(HeatPerStat=40)는 고정.
[Pool(typeof(ShumitCardPool))]
public sealed class BlacksmithsBlessingCard : ShumitCard
{
    private const string HeatPerStatKey = "HeatPerStat";
    private const string StatPerStackKey = "StatPerStack";

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar(HeatPerStatKey, 40m),
        new DynamicVar(StatPerStackKey, 1m),
    };

    public BlacksmithsBlessingCard()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heat = Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0;
        int per = System.Math.Max(1, DynamicVars[HeatPerStatKey].IntValue);
        int chunks = heat / per;
        if (chunks > 0)
        {
            decimal amt = chunks * DynamicVars[StatPerStackKey].BaseValue;
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, amt, Owner.Creature, this);
            await PowerCmd.Apply<DexterityPower>(Owner.Creature, amt, Owner.Creature, this);
        }

        if (heat > 0)
        {
            await FrontierHeatUtil.ApplyHeat(choiceContext, Owner.Creature, heat, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[StatPerStackKey].UpgradeValueBy(1m);
    }
}
