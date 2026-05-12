using System.Collections.Generic;
using Frontier.Cards;
using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using Frontier.Relics;
using Frontier.Resources;

namespace Frontier.Characters;

public sealed class ShumitCharacter : PlaceholderCharacterModel
{

	/// <summary>ModelDb/PrefixIdPatch 기준 캐릭터 엔트리 ID (characters 테이블 키 접두사와 동일).</summary>
	public const string CharacterId = "FRONTIER-SHUMIT_CHARACTER";

	public override Color NameColor => new Color("FFB74D");
	public override CharacterGender Gender => CharacterGender.Feminine;
	public override int StartingHp => 70;

	/// <summary>
	/// 해금 조건: 디펙트로 런 1회 완료 후 해금. 잠금 안내 문구의 {Prerequisite} 변수에 사용된다.
	/// 실제 잠금 여부는 <see cref="Frontier.Patches.FrontierShumitUnlockPatch"/> 에서
	/// <c>Defect1Epoch</c> 진행 여부로 결정한다.
	/// </summary>
	protected override CharacterModel? UnlocksAfterRunAs => ModelDb.Character<Defect>();

	// CustomCharacterSelectLockedIconPath 는 의도적으로 오버라이드하지 않는다.
	// PCK 에 ctex 가 없는 PNG 경로를 주면 ResourceLoader.Load 가 실패한다.
	// 잠금 상태 슈미트 아이콘은 FrontierShumitCharSelectVisualPatches.ApplyShumitIcons 가
	// Init 끝난 뒤 PNG 를 직접 읽어 교체한다.

	/// <summary>보상/상점 등 캐릭터 카드 풀. Ironclad 풀을 쓰면 슈미트 카드가 보상에 나오지 않으므로 전용 풀을 둔다.</summary>
	public override CardPoolModel CardPool => ModelDb.CardPool<ShumitCardPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<IroncladRelicPool>();
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<IroncladPotionPool>();

	/// <summary>
	/// 시작 덱: README·기획 기준 타격×4, 수비×4, 단조×1, 유냉×1 (총 10장).
	/// <see cref="ForgeCard"/>는 시작 덱에 넣지 않는다. <see cref="BrokenForgeRelic"/>이 전투 시작 시 뽑을 더미에 추가한다.
	/// 카드 구현 패턴은 <c>unpack/docs/cards-system-analysis.md</c>를 참고한다.
	/// </summary>
	public override IEnumerable<CardModel> StartingDeck => CreateStartingDeck();

	private static IEnumerable<CardModel> CreateStartingDeck()
	{
		return new CardModel[]
		{
			ModelDb.Card<StrikeShumitCard>(),
			ModelDb.Card<StrikeShumitCard>(),
			ModelDb.Card<StrikeShumitCard>(),
			ModelDb.Card<StrikeShumitCard>(),
			ModelDb.Card<DefendShumitCard>(),
			ModelDb.Card<DefendShumitCard>(),
			ModelDb.Card<DefendShumitCard>(),
			ModelDb.Card<DefendShumitCard>(),
			ModelDb.Card<ForgingCard>(),
			ModelDb.Card<OilCoolingCard>()
		};
	}

	public override IReadOnlyList<RelicModel> StartingRelics => new RelicModel[1] { ModelDb.Relic<BrokenForgeRelic>() };
	public override string PlaceholderID => "ironclad";

	/// <summary>
	/// 전투 상단 등 커스텀 아이콘 — <c>ResourceLoader</c> 대신 PNG 바이너리 로드(PCK에 원본 PNG만 있어도 됨).
	/// 캐릭터 선택 버튼·배경은 <c>FrontierShumitCharSelectVisualPatches</c> 가 동일 PNG로 덮어쓴다.
	/// </summary>
	public override Control CustomIcon
	{
		get
		{
			Texture2D? tex = FrontierResPngTexture.TryLoadTexture2DFromRes(ShumitCharUiPaths.TopBarIcon);
			if (tex != null)
			{
				TextureRect tr = new();
				tr.Texture = tex;
				tr.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				tr.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
				return tr;
			}

			Control fallback = NodeFactory<Control>.CreateFromResource("res://images/ui/top_panel/character_icon_ironclad.png");
			fallback.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			return fallback;
		}
	}

}

