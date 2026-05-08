using System.Collections.Generic;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using Frontier.Relics;
using Frontier.Cards.Starter;

namespace Frontier.Characters;

public sealed class ShumitCharacter : PlaceholderCharacterModel
{
	public const string CharacterId = "Shumit";

	public override Color NameColor => new Color("A5D6FF");
	public override CharacterGender Gender => CharacterGender.Masculine;
	public override int StartingHp => 72;
	public override CardPoolModel CardPool => ModelDb.CardPool<IroncladCardPool>();
	public override RelicPoolModel RelicPool => ModelDb.RelicPool<IroncladRelicPool>();
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<IroncladPotionPool>();

	public override IEnumerable<CardModel> StartingDeck => new CardModel[10]
	{
		ModelDb.Card<StrikeIronclad>(),
		ModelDb.Card<StrikeIronclad>(),
		ModelDb.Card<StrikeIronclad>(),
		ModelDb.Card<StrikeIronclad>(),
		ModelDb.Card<DefendIronclad>(),
		ModelDb.Card<DefendIronclad>(),
		ModelDb.Card<DefendIronclad>(),
		ModelDb.Card<DefendIronclad>(),
		ModelDb.Card<ForgingCard>(),
		ModelDb.Card<OilCoolingCard>()
	};

	public override IReadOnlyList<RelicModel> StartingRelics => new RelicModel[1] { ModelDb.Relic<BrokenForgeRelic>() };
	public override string PlaceholderID => "ironclad";

	public override List<(string, string)> Localization => new List<(string, string)>
	{
		("title", "Shumit"),
		("titleObject", "Shumit"),
		("description", "One of the pioneers dragged into another world. He raises his hammer and carves a path to return to his home world."),
		("aromaPrinciple", "[sine][orange]Steel remembers every strike.[/orange][/sine]"),
		("banter.alive.endTurnPing", "One more strike."),
		("banter.dead.endTurnPing", "...Not done yet."),
		("goldMonologue", "[sine]Enough iron, and I can forge a road home.[/sine]"),
		("pronounObject", "him"),
		("pronounPossessive", "his"),
		("pronounSubject", "he"),
		("possessiveAdjective", "his"),
		("cardsModifierTitle", "Shumit Cards"),
		("cardsModifierDescription", "Shumit cards will now appear in rewards and shops."),
		("eventDeathPrevention", "Shumit endured this trial."),
		("unlockText", "A blacksmith from another world, forging his way home.")
	};
}
