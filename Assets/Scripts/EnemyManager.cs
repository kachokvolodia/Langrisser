using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void DoAllEnemiesTurn(System.Action onEnemiesFinished)
    {
        StartCoroutine(EnemyTurnRoutine(onEnemiesFinished));
    }

    IEnumerator<WaitForSeconds> EnemyTurnRoutine(System.Action onEnemiesFinished)
    {
        UnitManager.Instance.ApplyWaitHealing();

        foreach (var enemy in UnitManager.Instance.AllUnits)
        {
            if (enemy.faction != Unit.Faction.Enemy)
                continue;

            yield return new WaitForSeconds(0.5f);

            // Проверяем наличие EnemyAI!
            var ai = enemy.GetComponent<EnemyAI>();
            if (ai != null)
                ai.DoEnemyTurn();
        }


        yield return new WaitForSeconds(0.5f);
        onEnemiesFinished?.Invoke();
    }
    public Unit FindNearestPlayerUnit(Unit enemy)
    {
        Unit nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var unit in UnitManager.Instance.AllUnits)
        {
            if (unit.faction != Unit.Faction.Player)
                continue;
            float dist = Vector2.Distance(enemy.transform.position, unit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = unit;
            }
        }
        return nearest;
    }
}
