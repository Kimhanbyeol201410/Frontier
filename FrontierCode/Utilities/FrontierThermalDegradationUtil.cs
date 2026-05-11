using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Frontier.Utilities;

/// <summary>열화: 강화 1단계 제거. <see cref="CardCmd.Downgrade"/> 오버로드 중 <see cref="PlayerChoiceContext"/>·<see cref="CardModel"/>·<see cref="CardPreviewStyle"/> 조합만 지원한다.</summary>
internal static class FrontierThermalDegradationUtil
{
	private static MethodInfo? _downgrade;
	private static bool _resolved;
	private static bool _warned;

	internal static async Task TryApplyOneStep(PlayerChoiceContext choiceContext, CardModel card)
	{
		if (card.CurrentUpgradeLevel <= 0)
		{
			return;
		}

		if (!_resolved)
		{
			_resolved = true;
			MethodInfo[] candidates = typeof(CardCmd).GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(static m => m.Name == "Downgrade")
				.ToArray();
			foreach (MethodInfo m in candidates.OrderBy(static m => m.GetParameters().Length))
			{
				if (CanBuildArgs(m))
				{
					_downgrade = m;
					break;
				}
			}

			if (_downgrade == null && !_warned)
			{
				_warned = true;
				GD.PrintErr("[Frontier] CardCmd.Downgrade 를 찾거나 인식하지 못했습니다. 열화가 적용되지 않습니다.");
			}
		}

		if (_downgrade == null)
		{
			return;
		}

		if (!TryBuildArgs(_downgrade, choiceContext, card, out object?[]? args))
		{
			return;
		}

		object? ret = _downgrade.Invoke(null, args);
		if (ret is Task task)
		{
			await task;
		}
	}

	private static bool CanBuildArgs(MethodInfo method)
	{
		return method.GetParameters().All(static p =>
			p.ParameterType == typeof(PlayerChoiceContext)
			|| p.ParameterType == typeof(CardModel)
			|| p.ParameterType == typeof(CardPreviewStyle));
	}

	private static bool TryBuildArgs(MethodInfo method, PlayerChoiceContext choiceContext, CardModel card, out object?[]? args)
	{
		ParameterInfo[] ps = method.GetParameters();
		args = new object?[ps.Length];
		for (int i = 0; i < ps.Length; i++)
		{
			Type pt = ps[i].ParameterType;
			if (pt == typeof(PlayerChoiceContext))
			{
				args[i] = choiceContext;
			}
			else if (pt == typeof(CardModel))
			{
				args[i] = card;
			}
			else if (pt == typeof(CardPreviewStyle))
			{
				args[i] = CardPreviewStyle.HorizontalLayout;
			}
			else
			{
				args = null;
				return false;
			}
		}

		return true;
	}
}
