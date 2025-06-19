using UnityEngine;

/// <summary>
/// AI фракции Moon Archon Dominion. Эльфы действуют из личной выгоды и готовы
/// жертвовать союзниками ради убийства врага.
/// </summary>
public class MoonArchonDominionAI : BaseFactionAI
{
    protected override void DoCommanderLogic(Unit me)
    {
        // Командир всегда атакует ближайшую цель, игнорируя опасность
        base.DoCommanderLogic(me);
    }

    protected override void DoSoldierLogic(Unit me)
    {
        Unit target = FindBestEnemyTarget(me);
        if (target != null && InAttackRange(me, target))
        {
            // Атакуем даже если можем погибнуть
            UnitManager.Instance.ResolveCombat(me, target);
            return;
        }
        if (target != null)
        {
            MoveTowardsTarget(me, target);
            return;
        }
    }
}
