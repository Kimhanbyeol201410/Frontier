using BaseLib.Abstracts;
using Frontier.Cards;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Frontier.Characters;

/// <summary>
/// 슈미트 캐릭터 전용 카드 풀. BaseLib <see cref="PoolAttribute"/>로 등록된 슈미트 카드가 이 풀에 속한다.
/// </summary>
public sealed class ShumitCardPool : CustomCardPoolModel
{
	public override string Title => ShumitCharacter.CharacterId;

	public override Color DeckEntryCardColor => new Color("A5D6FF");

	public override bool IsColorless => false;

	public override float H => 0.58f;

	public override float S => 0.35f;

	public override float V => 0.92f;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[]
		{
			ModelDb.Card<AbsoluteZeroCard>(),
			ModelDb.Card<AgileSmeltingCard>(),
			ModelDb.Card<AnvilEchoCard>(),
			ModelDb.Card<AnvilMemoryCard>(),
			ModelDb.Card<ApproachingDreadCard>(),
			ModelDb.Card<BellowsCard>(),
			ModelDb.Card<BlastFurnaceCard>(),
			ModelDb.Card<BurningStrikeCard>(),
			ModelDb.Card<ColdBurnStatusCard>(),
			ModelDb.Card<ColdGestureCard>(),
			ModelDb.Card<ColdHeartCard>(),
			ModelDb.Card<CoolingStrikeCard>(),
			ModelDb.Card<CoolingSystemCard>(),
			ModelDb.Card<CrushingHammerCard>(),
			ModelDb.Card<DefendShumitCard>(),
			ModelDb.Card<DesignCompletionCard>(),
			ModelDb.Card<DoubleTapCard>(),
			ModelDb.Card<EchoSmeltingCard>(),
			ModelDb.Card<ExhaustSystemCard>(),
			ModelDb.Card<FaultBreakCard>(),
			ModelDb.Card<FearlessOfFlameCard>(),
			ModelDb.Card<FirePowerPlantCard>(),
			ModelDb.Card<FlameArmorCard>(),
			ModelDb.Card<FlamePulseCard>(),
			ModelDb.Card<FlameSmashCard>(),
			ModelDb.Card<FlameStrikeCard>(),
			ModelDb.Card<FoldedSteelCard>(),
			ModelDb.Card<ForgeBlueprintCard>(),
			ModelDb.Card<ForgeCard>(),
			ModelDb.Card<ForgeFacilityBlueprintCard>(),
			ModelDb.Card<ForgingCard>(),
			ModelDb.Card<FurnaceMaintenanceCard>(),
			ModelDb.Card<GreatForgeCard>(),
			ModelDb.Card<GrindingRoomCard>(),
			ModelDb.Card<HammerDownCard>(),
			ModelDb.Card<HeartOfFlameCard>(),
			ModelDb.Card<HeatCycleCard>(),
			ModelDb.Card<HeatExchangeCard>(),
			ModelDb.Card<HeatedForgeCard>(),
			ModelDb.Card<HeatedHammerCard>(),
			ModelDb.Card<HeatedShieldCard>(),
			ModelDb.Card<HeatingCard>(),
			ModelDb.Card<ImpurityCompressionCard>(),
			ModelDb.Card<ImpurityRemovalCard>(),
			ModelDb.Card<IronArmorCard>(),
			ModelDb.Card<IronShieldCard>(),
			ModelDb.Card<IronSwordCard>(),
			ModelDb.Card<ManufactureCard>(),
			ModelDb.Card<MasterpieceHammerCard>(),
			ModelDb.Card<MasterPrideCard>(),
			ModelDb.Card<MasterySmeltingCard>(),
			ModelDb.Card<MaterialGatherCard>(),
			ModelDb.Card<MeltCard>(),
			ModelDb.Card<MeltingCard>(),
			ModelDb.Card<MetalLiquefactionCard>(),
			ModelDb.Card<OilCoolingCard>(),
			ModelDb.Card<RefiningCard>(),
			ModelDb.Card<ReuseAestheticsCard>(),
			ModelDb.Card<SharpSmeltingCard>(),
			ModelDb.Card<SmelterCard>(),
			ModelDb.Card<SmeltingDesignCard>(),
			ModelDb.Card<SmeltingStrikeCard>(),
			ModelDb.Card<SparkBurstCard>(),
			ModelDb.Card<SteamReleaseCard>(),
			ModelDb.Card<StrikeShumitCard>(),
			ModelDb.Card<SturdyArmorCard>(),
			ModelDb.Card<TapCard>(),
			ModelDb.Card<TemporaryQuenchingCard>(),
			ModelDb.Card<ThermalEnergyConversionCard>(),
			ModelDb.Card<TongsCard>(),
			ModelDb.Card<UnburningBodyCard>(),
			ModelDb.Card<UnstoppableHeatCard>(),
			ModelDb.Card<UntilExhaustionCard>(),
			ModelDb.Card<WaterCoolingCard>()
		};
	}
}
