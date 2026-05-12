using System.Collections.Generic;
using BaseLib.Abstracts;
using Frontier.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Frontier.Characters;

/// <summary>
/// 슈미트 캐릭터 전용 유물 풀. 일반 전투 보상에서 슈미트 전용 유물이 등장하려면
/// 캐릭터의 <see cref="ShumitCharacter.RelicPool"/> 이 이 풀을 가리키고,
/// 풀에 포함될 유물에 <c>[Pool(typeof(ShumitRelicPool))]</c> 가 붙어 있어야 한다.
/// </summary>
public sealed class ShumitRelicPool : CustomRelicPoolModel
{
    public override string? BigEnergyIconPath => "res://images/atlases/ui_atlas.sprites/card/energy_ironclad.tres";

    public override string? TextEnergyIconPath => "res://images/packed/sprite_fonts/ironclad_energy_icon.png";

    protected override IEnumerable<RelicModel> GenerateAllRelics()
    {
        return new RelicModel[]
        {
            ModelDb.Relic<HeatproofApronRelic>(),
            ModelDb.Relic<HephaestusBloodRelic>(),
            ModelDb.Relic<EndlessLaborRelic>(),
            ModelDb.Relic<UnburnableBodyRelic>(),
            ModelDb.Relic<MasterpieceMuseumRelic>(),
            ModelDb.Relic<EternallyBurningFurnaceRelic>(),
        };
    }
}
