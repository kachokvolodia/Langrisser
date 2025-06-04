using UnityEngine;
using System.Collections;
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

    IEnumerator EnemyTurnRoutine(System.Action onEnemiesFinished)
    {
        // Лечим вражеские и нейтральные юниты, которые бездействовали в прошлый ход
        UnitManager.Instance.ApplyWaitHealing(
            Unit.Faction.Enemy,
            Unit.Faction.EnemyAlly,
            Unit.Faction.Neutral,
            Unit.Faction.EvilNeutral);

        // Сбрасываем флаги действий у этих юнитов перед их ходом
        UnitManager.Instance.ResetUnitsForNextTurn(
            Unit.Faction.Enemy,
            Unit.Faction.EnemyAlly,
            Unit.Faction.Neutral,
            Unit.Faction.EvilNeutral);

        // Создаем копию списка, так как во время хода юниты могут погибать
        var enemiesSnapshot = new List<Unit>(UnitManager.Instance.AllUnits);
        foreach (var enemy in enemiesSnapshot)
        {
            // Игрок управляет лишь своей фракцией
            if (enemy == null || enemy.faction == Unit.Faction.Player)
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
