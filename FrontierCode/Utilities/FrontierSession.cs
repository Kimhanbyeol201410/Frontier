namespace Frontier.Utilities;

/// <summary>전투/턴 단위 추적(도면 카드, 소멸한 화상 수 등).</summary>
internal static class FrontierSession
{
	internal static int UpgradesThisCombat { get; private set; }

	internal static int BurnsExhaustedThisPlayerTurn { get; private set; }

	internal static bool BurnCardExhaustedThisPlayerTurn { get; private set; }

	internal static void RegisterUpgrade()
	{
		UpgradesThisCombat++;
	}

	internal static void RegisterBurnExhausted()
	{
		BurnsExhaustedThisPlayerTurn++;
		BurnCardExhaustedThisPlayerTurn = true;
	}

	internal static void OnCombatStarted()
	{
		UpgradesThisCombat = 0;
		BurnsExhaustedThisPlayerTurn = 0;
		BurnCardExhaustedThisPlayerTurn = false;
	}

	internal static void OnPlayerTurnEnded()
	{
		BurnsExhaustedThisPlayerTurn = 0;
		BurnCardExhaustedThisPlayerTurn = false;
	}
}
