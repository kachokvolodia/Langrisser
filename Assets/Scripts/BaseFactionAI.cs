using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Базовый класс для AI фракций. Содержит общие вспомогательные методы и
/// стандартное поведение, которое могут переопределять потомки.
/// </summary>
public abstract class BaseFactionAI : MonoBehaviour, IUnitAI
{
    public void DoEnemyTurn()
    {
        Unit me = GetComponent<Unit>();
        if (me == null || me.hasActed) return;

        if (me.isCommander)
            DoCommanderLogic(me);
        else
            DoSoldierLogic(me);
    }

    protected virtual void DoCommanderLogic(Unit me)
    {
        // Стандартное поведение командира: попытка атаковать ближайшего врага
        Unit target = FindBestEnemyTarget(me);
        if (target != null && InAttackRange(me, target))
        {
            UnitManager.Instance.ResolveCombat(me, target);
            return;
        }
        if (target != null)
        {
            MoveTowardsTarget(me, target);
        }
    }

    protected virtual void DoSoldierLogic(Unit me)
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
    }

    // === Вспомогательные методы ===
    protected Unit FindBestEnemyTarget(Unit me)
    {
        var allUnits = UnitManager.Instance.AllUnits;
        Unit best = null;
        float bestScore = float.NegativeInfinity;
        foreach (var u in allUnits)
        {
            if (u == null || u == me || u.currentHP <= 0) continue;
            var rel = FactionManager.Instance != null ?
                FactionManager.Instance.GetRelation(me.faction, u.faction) :
                FactionManager.RelationType.Neutral;
            if (rel != FactionManager.RelationType.Enemy) continue;

            float dist = Vector2.Distance(me.transform.position, u.transform.position);
            float score = -dist;
            if (u.isCommander) score += 5f;
            if (score > bestScore)
            {
                bestScore = score;
                best = u;
            }
        }
        return best;
    }

    protected bool InAttackRange(Unit attacker, Unit target)
    {
        Vector2Int attPos = GridManager.Instance.WorldToGrid(attacker.transform.position);
        Vector2Int tgtPos = GridManager.Instance.WorldToGrid(target.transform.position);
        int dx = Mathf.Abs(attPos.x - tgtPos.x);
        int dy = Mathf.Abs(attPos.y - tgtPos.y);
        int dist = dx + dy;
        int range = attacker.GetAttackRange();

        if (range == 1)
            return dist == 1 && (dx == 1 ^ dy == 1);
        else
            return dist <= range;
    }

    protected Cell FindAdjacentFreeCell(Unit me, Unit target)
    {
        var grid = GridManager.Instance;
        Vector2Int[] deltas = { new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(-1,0)};
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

    protected void MoveTowardsTarget(Unit me, Unit target)
    {
        int attackRange = me.GetAttackRange();
        if (attackRange == 1)
        {
            Cell goal = FindAdjacentFreeCell(me, target);
            if (goal == null) return;
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
                        if (oldCell != null) oldCell.occupyingUnit = null;
                        me.MoveTo(cell.worldPos);
                        cell.occupyingUnit = me;
                    }
                    else break;
                }
            }
        }
        else
        {
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
                        if (oldCell != null) oldCell.occupyingUnit = null;
                        me.MoveTo(cell.worldPos);
                        cell.occupyingUnit = me;
                    }
                    else break;
                }
            }
        }
    }
}
