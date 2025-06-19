using UnityEngine;
using System.Linq;

/// <summary>
/// AI для враждебных нейтралов. Предпочитают охотиться стаей и сбиваться вместе.
/// </summary>
public class EvilNeutralAI : BaseFactionAI
{
    protected override void DoSoldierLogic(Unit me)
    {
        Unit target = FindBestEnemyTarget(me);
        if (target != null && InAttackRange(me, target))
        {
            UnitManager.Instance.ResolveCombat(me, target);
            return;
        }
        if (target != null)
        {
            MoveTowardsTarget(me, target);
            return;
        }

        // Если врага нет, стараемся держаться рядом с другими злыми нейтралами
        Unit ally = FindNearestAlly(me);
        if (ally != null)
        {
            MoveTowardsTarget(me, ally);
        }
    }

    private Unit FindNearestAlly(Unit me)
    {
        return UnitManager.Instance.AllUnits
            .Where(u => u != null && u != me && u.faction == me.faction)
            .OrderBy(u => Vector2.Distance(u.transform.position, me.transform.position))
            .FirstOrDefault();
    }
}
