using UnityEngine;
using System.Linq;

/// <summary>
/// AI фракции Aurora Empire. Рыцари чести, стараются защищать командиров и не
/// рискуют напрасно.
/// </summary>
public class AuroraEmpireAI : BaseFactionAI
{
    protected override void DoCommanderLogic(Unit me)
    {
        // Если рядом союзный командир с низким здоровьем, идём к нему на помощь
        Unit allyCmd = FindWoundedCommander(me);
        if (allyCmd != null && allyCmd != me)
        {
            MoveTowardsTarget(me, allyCmd);
            return;
        }
        base.DoCommanderLogic(me);
    }

    protected override void DoSoldierLogic(Unit me)
    {
        // Сначала прикрываем своего или союзного командира
        Unit allyCmd = FindWoundedCommander(me);
        if (allyCmd != null && Vector2.Distance(me.transform.position, allyCmd.transform.position) > 1f)
        {
            MoveTowardsTarget(me, allyCmd);
            return;
        }

        // Перед атакой проверим, не будет ли самоубийства
        Unit target = FindBestEnemyTarget(me);
        if (target != null && InAttackRange(me, target))
        {
            int dmgToEnemy = me.CalculateDamage(target);
            int dmgFromEnemy = target.CalculateDamage(me);
            bool killable = dmgToEnemy >= target.currentHP;
            if (dmgFromEnemy >= me.currentHP && !killable)
            {
                // Лучше отступить к командиру
                if (me.commander != null)
                    MoveTowardsTarget(me, me.commander);
                return;
            }
            StartCoroutine(UnitManager.Instance.ResolveCombat(me, target));
            return;
        }
        base.DoSoldierLogic(me);
    }

    private Unit FindWoundedCommander(Unit me)
    {
        return UnitManager.Instance.AllUnits
            .Where(u => u != null && u.isCommander && u.faction == me.faction)
            .OrderBy(u => (float)u.currentHP / u.MaxHP)
            .FirstOrDefault(u => (float)u.currentHP / u.MaxHP < 0.75f);
    }
}
