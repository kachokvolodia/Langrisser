using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// AI фракции Золотой Руки. Старается держаться вместе и поддерживать союзников.
/// </summary>
public class GoldenHandAI : BaseFactionAI
{
    protected override void DoSoldierLogic(Unit me)
    {
        // Если рядом меньше двух союзников - держимся ближе к командиру
        int allyCount = CountAlliesAround(me, 1);
        if (allyCount < 2 && me.commander != null && !me.IsInAura())
        {
            MoveTowardsTarget(me, me.commander);
            return;
        }

        base.DoSoldierLogic(me);
    }

    private int CountAlliesAround(Unit me, int radius)
    {
        return UnitManager.Instance.AllUnits.Count(u => u != null && u != me && u.faction == me.faction &&
            Vector2.Distance(u.transform.position, me.transform.position) <= radius);
    }
}
