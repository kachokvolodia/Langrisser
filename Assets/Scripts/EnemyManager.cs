using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void DoFactionTurn(Faction faction, System.Action onFinished)
    {
        StartCoroutine(FactionTurnRoutine(faction, onFinished));
    }

    IEnumerator FactionTurnRoutine(Faction faction, System.Action onFinished)
    {
        UnitManager.Instance.ApplyWaitHealing(faction);
        UnitManager.Instance.ResetUnitsForNextTurn(faction);

        var snapshot = new List<Unit>(UnitManager.Instance.AllUnits);
        foreach (var unit in snapshot)
        {
            if (unit == null || unit.faction != faction)
                continue;

            yield return new WaitForSeconds(0.5f);

            if (unit == null)
                continue;

            var ai = unit.GetComponent<IUnitAI>();
            if (ai != null)
                ai.DoEnemyTurn();
        }

        yield return new WaitForSeconds(0.5f);
        onFinished?.Invoke();
    }
    public Unit FindNearestPlayerUnit(Unit enemy)
    {
        Unit nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var unit in UnitManager.Instance.AllUnits)
        {
            if (unit.faction != FactionManager.PlayerFaction)
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
