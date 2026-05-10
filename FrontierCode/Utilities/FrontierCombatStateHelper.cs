using System;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Frontier.Utilities;

/// <summary>
/// 일부 환경에서 <c>Creature.CombatState</c> 프로퍼티 접근 시 서명 불일치(<see cref="MissingMethodException"/>)가 나므로,
/// 활성 전투는 <see cref="CombatManager.DebugOnlyGetState"/>로 조회합니다.
/// </summary>
public static class FrontierCombatStateHelper
{
    public static CombatState? TryGetFor(Player? owner)
    {
        CombatState? state = CombatManager.Instance.DebugOnlyGetState();
        if (state == null || owner == null)
        {
            return null;
        }

        return state.Players.Contains(owner) ? state : null;
    }

    /// <summary>플레이어·펫·적 등 전투 참가 크리처에 대해 현재 <see cref="CombatState"/>를 찾습니다.</summary>
    public static CombatState? TryGetForCreature(Creature? creature)
    {
        if (creature == null)
        {
            return null;
        }

        CombatState? state = CombatManager.Instance.DebugOnlyGetState();
        if (state == null)
        {
            return null;
        }

        if (creature.Player != null)
        {
            return state.Players.Contains(creature.Player) ? state : null;
        }

        if (creature.PetOwner != null)
        {
            return state.Players.Contains(creature.PetOwner) ? state : null;
        }

        return state.ContainsCreature(creature) ? state : null;
    }

    public static CombatState RequireFor(Player owner)
    {
        return TryGetFor(owner)
            ?? throw new InvalidOperationException("Active CombatState required for this player.");
    }
}
