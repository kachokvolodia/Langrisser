using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyAI : MonoBehaviour
{
    private Cell FindFreeAdjacentToCommander(Unit me)
    {
        if (me.commander == null) return null;
        var grid = GridManager.Instance;
        Vector2Int[] deltas = {
        new Vector2Int(0, 1), new Vector2Int(1, 0),
        new Vector2Int(0, -1), new Vector2Int(-1, 0)
    };
        Vector2Int cmdPos = grid.WorldToGrid(me.commander.transform.position);

        foreach (var delta in deltas)
        {
            Vector2Int pos = cmdPos + delta;
            if (pos.x >= 0 && pos.x < grid.Width && pos.y >= 0 && pos.y < grid.Height)
            {
                var cell = grid.cells[pos.x, pos.y];
                if (cell.occupyingUnit == null)
                    return cell;
            }
        }
        return null;
    }

    public void DoEnemyTurn()
    {
        Unit me = GetComponent<Unit>();
        if (me == null || me.hasActed) return;

        // 1. Если я командир
        if (me.isCommander)
        {
            DoCommanderLogic(me);
        }
        else
        {
            DoSoldierLogic(me);
        }
    }

    // === Логика для командира ===
    private void DoCommanderLogic(Unit me)
    {
        float hpPercent = (float)me.currentHP / me.MaxHP;

        // 1. Если сильно ранен и рядом нет врага — стоим, чтобы хилиться
        Unit closeEnemy = FindBestEnemyTarget(me);
        bool enemyNear = (closeEnemy != null && InAttackRange(me, closeEnemy));
        if (hpPercent < 0.5f && !enemyNear)
        {
            Debug.Log($"{me.unitData.unitName} (командир) тяжело ранен и хилится, не двигается.");
            return; // Стоит на месте!
        }

        // 2. Если враг в диапазоне — атакуем
        if (closeEnemy != null && InAttackRange(me, closeEnemy))
        {
            UnitManager.Instance.ResolveCombat(me, closeEnemy);
            Debug.Log($"{me.unitData.unitName} (командир) атакует {closeEnemy.unitData.unitName}");
            return;
        }

        // 3. Если враг есть, но не достаем — идём к нему
        if (closeEnemy != null)
        {
            MoveTowardsTarget(me, closeEnemy);
            return;
        }

        // 4. Если врагов вообще нет — просто стоим
        Debug.Log($"{me.unitData.unitName} (командир) не нашёл врагов");
    }

    // === Логика для солдата ===
    private void DoSoldierLogic(Unit me)
    {
        // 1. Если сильно ранен или командир ранен — идём к командиру для хила
        bool selfWounded = (float)me.currentHP / me.MaxHP < 0.7f;
        bool commanderWounded = (me.commander != null && (float)me.commander.currentHP / me.commander.MaxHP < 0.5f);
        bool tryHeal = selfWounded || commanderWounded;

        if (me.commander != null && tryHeal)
        {
            // Проверим: уже стоим крестом рядом? Если да — стоим, ждём хил.
            Vector2Int myPos = GridManager.Instance.WorldToGrid(me.transform.position);
            Vector2Int cmdPos = GridManager.Instance.WorldToGrid(me.commander.transform.position);
            int dx = Mathf.Abs(myPos.x - cmdPos.x);
            int dy = Mathf.Abs(myPos.y - cmdPos.y);

            if (dx + dy == 1)
            {
                Debug.Log($"{me.unitData.unitName} ждёт хил рядом с командиром.");
                return; // Остаёмся ждать хила!
            }
            // --- ВНИМАНИЕ: цикл всегда после return, не внутри блока if ---
            // Иначе идём к ближайшей свободной крестовой клетке рядом с командиром
            Cell targetCell = FindFreeAdjacentToCommander(me);
            if (targetCell != null)
            {
                var startCell = UnitManager.Instance.GetCellOfUnit(me);
                var path = PathfindingManager.Instance.FindPath(startCell, targetCell, me);
                if (path != null && path.Count > 1)
                {
                    for (int i = 1; i < path.Count && i <= me.unitData.moveRange; i++)
                    {
                        var cell = path[i];
                        if (cell.occupyingUnit == null)
                        {
                            var oldCell = UnitManager.Instance.GetCellOfUnit(me);
                            if (oldCell != null) oldCell.occupyingUnit = null;

                            me.MoveTo(cell.worldPos);
                            cell.occupyingUnit = me;
                            Debug.Log($"{me.unitData.unitName} идёт к командиру для хила!");
                            return;
                        }
                        else break;
                    }
                }
            }
        }


            // 2. Как обычно: ищем врага в радиусе
            Unit target = FindBestEnemyTarget(me);

        // 3. Если враг в радиусе атаки — атакуем
        if (target != null && InAttackRange(me, target))
        {
            UnitManager.Instance.ResolveCombat(me, target);
            Debug.Log($"{me.unitData.unitName} атакует {target.unitData.unitName}");
            return;
        }

        // 4. Если враг есть, но не в радиусе — двигаемся к нему, только если не надо догонять командира
        if (target != null && me.commander != null && me.IsInAura())
        {
            MoveTowardsTarget(me, target);
            Debug.Log($"{me.unitData.unitName} движется к врагу в ауре командира");
            return;
        }

        // 5. Если далеко от командира — догоняем его (держимся вместе)
        if (me.commander != null && !me.IsInAura())
        {
            MoveTowardsTarget(me, me.commander);
            Debug.Log($"{me.unitData.unitName} догоняет своего командира");
            return;
        }

        // 6. Если ничего не надо — стоим на месте
        Debug.Log($"{me.unitData.unitName} стоит на месте (нет врага/не в ауре)");
    }


    // === Вспомогательные методы ===

    private Unit FindBestEnemyTarget(Unit me)
    {
        var allUnits = UnitManager.Instance.AllUnits;
        Unit best = null;
        float bestScore = float.NegativeInfinity;

        foreach (var u in allUnits)
        {
            if (u == null || u == me || u.currentHP <= 0) continue;

            var rel = FactionManager.Instance != null
                ? FactionManager.Instance.GetRelation(me.faction, u.faction)
                : FactionManager.RelationType.Neutral;
            if (rel != FactionManager.RelationType.Enemy) continue;

            float dist = Vector2.Distance(me.transform.position, u.transform.position);
            int predictedDmg = me.CalculateDamage(u);
            bool killable = predictedDmg >= u.currentHP;

            float score = 0f;
            score -= dist;               // ближе — приоритетнее
            score -= u.currentHP * 0.1f; // раненые цели легче добить
            if (killable) score += 20f;  // большой бонус за возможное убийство
            if (u.isCommander) score += 10f; // командиры — приоритетные цели

            if (score > bestScore)
            {
                bestScore = score;
                best = u;
            }
        }

        return best;
    }

    private bool InAttackRange(Unit attacker, Unit target)
    {
        Vector2Int attPos = GridManager.Instance.WorldToGrid(attacker.transform.position);
        Vector2Int tgtPos = GridManager.Instance.WorldToGrid(target.transform.position);
        int dx = Mathf.Abs(attPos.x - tgtPos.x);
        int dy = Mathf.Abs(attPos.y - tgtPos.y);
        int dist = dx + dy;
        int range = attacker.GetAttackRange();

        if (range == 1)
            return dist == 1 && (dx == 1 ^ dy == 1); // строго по кресту, не по диагонали!
        else
            return dist <= range;
    }


    // Для ближников — ищем свободную клетку рядом с целью по кресту
    private Cell FindAdjacentFreeCell(Unit me, Unit target)
    {
        var grid = GridManager.Instance;
        Vector2Int[] deltas = {
        new Vector2Int(0, 1), new Vector2Int(1, 0),
        new Vector2Int(0, -1), new Vector2Int(-1, 0)
    };
        Vector2Int targetPos = grid.WorldToGrid(target.transform.position);
        List<Cell> candidates = new List<Cell>();

        foreach (var delta in deltas)
        {
            Vector2Int pos = targetPos + delta;
            if (pos.x >= 0 && pos.x < grid.Width && pos.y >= 0 && pos.y < grid.Height)
            {
                var cell = grid.cells[pos.x, pos.y];
                if (cell.occupyingUnit == null)
                    candidates.Add(cell);
            }
        }

        // Найди ближайшую к себе
        Cell best = null;
        float minDist = float.MaxValue;
        foreach (var c in candidates)
        {
            float dist = Vector2.Distance(me.transform.position, c.worldPos);
            if (dist < minDist)
            {
                minDist = dist;
                best = c;
            }
        }
        return best;
    }

    private void MoveTowardsTarget(Unit me, Unit target)
    {
        // Определяем тип атаки
        int attackRange = me.GetAttackRange();

        if (attackRange == 1)
        {
            // Для ближников — цель движения не сам враг, а соседняя клетка по кресту
            Cell goal = FindAdjacentFreeCell(me, target);

            if (goal == null)
            {
                Debug.Log($"{me.unitData.unitName}: нет свободных клеток у цели!");
                return;
            }

            Cell startCell = UnitManager.Instance.GetCellOfUnit(me);
            var path = PathfindingManager.Instance.FindPath(startCell, goal, me);

            if (path != null && path.Count > 1)
            {
                for (int i = 1; i < path.Count && i <= me.unitData.moveRange; i++)
                {
                    var cell = path[i];
                    if (cell.occupyingUnit == null)
                    {
                        var oldCell = UnitManager.Instance.GetCellOfUnit(me);
                        if (oldCell != null)
                            oldCell.occupyingUnit = null;

                        me.MoveTo(cell.worldPos);
                        cell.occupyingUnit = me;
                    }
                    else break;
                }
            }
        }
        else // Для дальников и магов
        {
            // Можно расширить — например, стараться держать дистанцию или избегать соседних с врагом клеток
            Cell startCell = UnitManager.Instance.GetCellOfUnit(me);
            Cell targetCell = UnitManager.Instance.GetCellOfUnit(target);
            var path = PathfindingManager.Instance.FindPath(startCell, targetCell, me);

            if (path != null && path.Count > 1)
            {
                for (int i = 1; i < path.Count && i <= me.unitData.moveRange; i++)
                {
                    var cell = path[i];
                    if (cell.occupyingUnit == null)
                    {
                        var oldCell = UnitManager.Instance.GetCellOfUnit(me);
                        if (oldCell != null)
                            oldCell.occupyingUnit = null;

                        me.MoveTo(cell.worldPos);
                        cell.occupyingUnit = me;
                    }
                    else break;
                }
            }
        }
    }
} 
