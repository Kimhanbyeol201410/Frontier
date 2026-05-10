using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Frontier.Utilities;

/// <summary>
/// 전투/턴 단위 추적(도면 카드, 소멸한 화상 수 등). 멀티에서는 <see cref="Player.NetId"/>별로 분리한다.
/// </summary>
internal static class FrontierSession
{
	private sealed class Bucket
	{
		internal int UpgradesThisCombat;
		internal int BurnsExhaustedThisPlayerTurn;
		internal bool BurnCardExhaustedThisPlayerTurn;
	}

	private static readonly Dictionary<ulong, Bucket> Buckets = new();

	private static Bucket GetOrCreate(ulong netId)
	{
		if (!Buckets.TryGetValue(netId, out Bucket? b))
		{
			b = new Bucket();
			Buckets[netId] = b;
		}

		return b;
	}

	internal static int GetUpgradesThisCombat(Player player)
	{
		return player != null && Buckets.TryGetValue(player.NetId, out Bucket? b) ? b.UpgradesThisCombat : 0;
	}

	internal static int GetBurnsExhaustedThisPlayerTurn(Player player)
	{
		return player != null && Buckets.TryGetValue(player.NetId, out Bucket? b) ? b.BurnsExhaustedThisPlayerTurn : 0;
	}

	internal static bool GetBurnCardExhaustedThisPlayerTurn(Player player)
	{
		return player != null && Buckets.TryGetValue(player.NetId, out Bucket? b) && b.BurnCardExhaustedThisPlayerTurn;
	}

	internal static void RegisterUpgrade(Player? owner)
	{
		if (owner == null)
		{
			return;
		}

		Bucket b = GetOrCreate(owner.NetId);
		b.UpgradesThisCombat++;
	}

	internal static void RegisterBurnExhausted(Player? owner)
	{
		if (owner == null)
		{
			return;
		}

		Bucket b = GetOrCreate(owner.NetId);
		b.BurnsExhaustedThisPlayerTurn++;
		b.BurnCardExhaustedThisPlayerTurn = true;
	}

	internal static void OnCombatStarted()
	{
		Buckets.Clear();
	}

	/// <summary>플레이어 턴 종료 직전(손패 버리기 전). 해당 플레이어의 턴 한정 카운터만 초기화.</summary>
	internal static void OnPlayerBeforeHandFlush(Player? player)
	{
		if (player == null)
		{
			return;
		}

		if (!Buckets.TryGetValue(player.NetId, out Bucket? b))
		{
			return;
		}

		b.BurnsExhaustedThisPlayerTurn = 0;
		b.BurnCardExhaustedThisPlayerTurn = false;
	}
}
