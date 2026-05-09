using System;
using System.Linq;
using Frontier.Characters;
using Frontier.Localization;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Frontier.Patches;

/// <summary>
/// 캐릭터 선택 프리로드는 Placeholder(아이언클래드) 리소스를 쓰고,
/// 표시만 <c>sts2-frontier/images/charui/*.png</c> 바이너리를 읽어 덮어쓴다 — Godot 임포트·PCK 없이도 동작.
/// </summary>
internal static class FrontierShumitCharSelectVisualPatches
{
	private static Control BuildPortraitBg(Texture2D texture, string nodeName)
	{
		Control root = new();
		root.Name = nodeName;
		root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		TextureRect tr = new();
		tr.Texture = texture;
		tr.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		tr.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
		root.AddChild(tr);
		return root;
	}

	private static void ReplaceBgContainer(Control? bgContainer, CharacterModel character, string pngResPath)
	{
		if (bgContainer == null || character is not ShumitCharacter)
		{
			return;
		}

		Texture2D? tex = FrontierResPngTexture.TryLoadTexture2DFromRes(pngResPath);
		if (tex == null)
		{
			return;
		}

		foreach (Node child in bgContainer.GetChildren())
		{
			bgContainer.RemoveChildSafely(child);
			child.QueueFreeSafely();
		}

		Control portrait = BuildPortraitBg(tex, character.Id.Entry + "_bg");
		bgContainer.AddChildSafely(portrait);
	}

	[HarmonyPatch(typeof(NCharacterSelectButton), nameof(NCharacterSelectButton.Init))]
	internal static class CharacterSelectButtonInitPostfix
	{
		[HarmonyPostfix]
		private static void ApplyShumitIcons(NCharacterSelectButton __instance, CharacterModel character)
		{
			if (character is not ShumitCharacter)
			{
				return;
			}

			string path = __instance.IsLocked
				? FrontierShumitCharUiPaths.SelectPortraitLocked
				: FrontierShumitCharUiPaths.SelectPortrait;
			Texture2D? tex = FrontierResPngTexture.TryLoadTexture2DFromRes(path);
			if (tex == null)
			{
				return;
			}

			TextureRect icon = __instance.GetNode<TextureRect>("%Icon");
			icon.Texture = tex;
		}
	}

	[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
	internal static class CharacterSelectScreenSelectPostfix
	{
		private static readonly Lazy<System.Reflection.FieldInfo> BgField = new(() =>
			AccessTools.Field(typeof(NCharacterSelectScreen), "_bgContainer"));

		[HarmonyPostfix]
		private static void ReplaceShumitBg(NCharacterSelectScreen __instance, NCharacterSelectButton charSelectButton, CharacterModel characterModel)
		{
			if (characterModel is not ShumitCharacter || charSelectButton.IsLocked)
			{
				return;
			}

			Control? bg = BgField.Value.GetValue(__instance) as Control;
			ReplaceBgContainer(bg, characterModel, FrontierShumitCharUiPaths.SelectPortrait);
		}
	}

	[HarmonyPatch(typeof(NCharacterSelectScreen), "OnLocalCharacterChangedForRandom")]
	internal static class CharacterSelectRandomBgPostfix
	{
		private static readonly Lazy<System.Reflection.FieldInfo> BgField = new(() =>
			AccessTools.Field(typeof(NCharacterSelectScreen), "_bgContainer"));

		[HarmonyPostfix]
		private static void ReplaceShumitBg(NCharacterSelectScreen __instance, CharacterModel characterModel)
		{
			if (characterModel is not ShumitCharacter)
			{
				return;
			}

			Control? bg = BgField.Value.GetValue(__instance) as Control;
			ReplaceBgContainer(bg, characterModel, FrontierShumitCharUiPaths.SelectPortrait);
		}
	}

	[HarmonyPatch(typeof(NMultiplayerLoadGameScreen), "AfterMultiplayerStarted")]
	internal static class MultiplayerLoadBgPostfix
	{
		private static readonly Lazy<System.Reflection.FieldInfo> BgField = new(() =>
			AccessTools.Field(typeof(NMultiplayerLoadGameScreen), "_bgContainer"));

		private static readonly Lazy<System.Reflection.FieldInfo> RunLobbyField = new(() =>
			AccessTools.Field(typeof(NMultiplayerLoadGameScreen), "_runLobby"));

		[HarmonyPostfix]
		private static void ReplaceShumitBg(NMultiplayerLoadGameScreen __instance)
		{
			LoadRunLobby? runLobby = RunLobbyField.Value.GetValue(__instance) as LoadRunLobby;
			if (runLobby?.Run?.Players == null)
			{
				return;
			}

			SerializablePlayer? sp = runLobby.Run.Players.FirstOrDefault(p => p.NetId == runLobby.NetService.NetId);
			if (sp?.CharacterId is not { } cid)
			{
				return;
			}

			CharacterModel byId = ModelDb.GetById<CharacterModel>(cid);
			if (byId is not ShumitCharacter)
			{
				return;
			}

			Control? bg = BgField.Value.GetValue(__instance) as Control;
			ReplaceBgContainer(bg, byId, FrontierShumitCharUiPaths.SelectPortrait);
		}
	}
}
