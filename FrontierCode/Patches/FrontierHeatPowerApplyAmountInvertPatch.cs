using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Frontier;
using Frontier.Powers;

namespace Frontier.Patches;

/// <summary>리버스 엔지니어링: <see cref="ShumitReverseEngineeringInvertHeatPower"/>가 있는 동안 <c>HeatPower</c> 스택 변화 부호를 반전합니다.</summary>
[HarmonyPatch]
internal static class FrontierHeatPowerApplyAmountInvertPatch
{
	private static readonly MethodBase? ApplyHeatPowerMethod = ResolveApplyHeatPowerMethod();

	private static MethodBase? ResolveApplyHeatPowerMethod()
	{
		foreach (MethodInfo m in typeof(PowerCmd).GetMethods(BindingFlags.Public | BindingFlags.Static))
		{
			if (m.Name != "Apply" || !m.IsGenericMethodDefinition)
			{
				continue;
			}

			ParameterInfo[] ps = m.GetParameters();
			if (ps.Length is < 4 or > 5 || ps[0].ParameterType != typeof(Creature) || ps[1].ParameterType != typeof(decimal))
			{
				continue;
			}

			try
			{
				return m.MakeGenericMethod(typeof(HeatPower));
			}
			catch (ArgumentException)
			{
				// 다른 TPower 전용 시그니처
			}
		}

		return null;
	}

	[HarmonyPrepare]
	private static bool Prepare() => ApplyHeatPowerMethod != null;

	[HarmonyTargetMethod]
	private static MethodBase TargetMethod() => ApplyHeatPowerMethod!;

	[HarmonyPrefix]
	public static void InvertSignedHeatDeltaIfActive(Creature __0, ref decimal __1)
	{
		if (__0 == null || __1 == 0m)
		{
			return;
		}

		if (__0.GetPower<ShumitReverseEngineeringInvertHeatPower>() == null)
		{
			return;
		}

		__1 = -__1;
	}
}
