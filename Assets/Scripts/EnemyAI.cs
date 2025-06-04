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
            if (pos.x >= 0 && pos.x < grid.width && pos.y >= 0 && pos.y < grid.height)
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

        // 1. ���� � ��������
        if (me.isCommander)
        {
            DoCommanderLogic(me);
        }
        else
        {
            DoSoldierLogic(me);
        }
    }

    // === ������ ��� ��������� ===
    private void DoCommanderLogic(Unit me)
    {
        float hpPercent = (float)me.currentHP / me.unitData.maxHP;

        // 1. ���� ������ ����� � ����� ��� ����� � �����, ����� ��������
        Unit closeEnemy = FindBestEnemyTarget(me);
        bool enemyNear = (closeEnemy != null && InAttackRange(me, closeEnemy));
        if (hpPercent < 0.5f && !enemyNear)
        {
            Debug.Log($"{me.unitData.unitName} (��������) ������ ����� � �������, �� ���������.");
            return; // ����� �� �����!
        }

        // 2. ���� ���� � ��������� � �������
        if (closeEnemy != null && InAttackRange(me, closeEnemy))
        {
            UnitManager.Instance.ResolveCombat(me, closeEnemy);
            Debug.Log($"{me.unitData.unitName} (��������) ������� {closeEnemy.unitData.unitName}");
            return;
        }

        // 3. ���� ���� ����, �� �� ������� � ��� � ����
        if (closeEnemy != null)
        {
            MoveTowardsTarget(me, closeEnemy);
            return;
        }

        // 4. ���� ������ ������ ��� � ������ �����
        Debug.Log($"{me.unitData.unitName} (��������) �� ����� ������");
    }

    // === ������ ��� ������� ===
    private void DoSoldierLogic(Unit me)
    {
        // 1. ���� ������ ����� ��� �������� ����� � ��� � ��������� ��� ����
        bool selfWounded = (float)me.currentHP / me.unitData.maxHP < 0.7f;
        bool commanderWounded = (me.commander != null && (float)me.commander.currentHP / me.commander.unitData.maxHP < 0.5f);
        bool tryHeal = selfWounded || commanderWounded;

        if (me.commander != null && tryHeal)
        {
            // ��������: ��� ����� ������� �����? ���� �� � �����, ��� ���.
            Vector2Int myPos = GridManager.Instance.WorldToGrid(me.transform.position);
            Vector2Int cmdPos = GridManager.Instance.WorldToGrid(me.commander.transform.position);
            int dx = Mathf.Abs(myPos.x - cmdPos.x);
            int dy = Mathf.Abs(myPos.y - cmdPos.y);

            if (dx + dy == 1)
            {
                Debug.Log($"{me.unitData.unitName} ��� ��� ����� � ����������.");
                return; // ������� ����� ����!
            }
            // --- ��������: ���� ������ ����� return, �� ������ ����� if ---
            // ����� ��� � ��������� ��������� ��������� ������ ����� � ����������
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

                            me.MoveTo(cell.transform.position);
                            cell.occupyingUnit = me;
                            Debug.Log($"{me.unitData.unitName} ��� � ��������� ��� ����!");
                            return;
                        }
                        else break;
                    }
                }
            }
        }


            // 2. ��� ������: ���� ����� � �������
            Unit target = FindBestEnemyTarget(me);

        // 3. ���� ���� � ������� ����� � �������
        if (target != null && InAttackRange(me, target))
        {
            UnitManager.Instance.ResolveCombat(me, target);
            Debug.Log($"{me.unitData.unitName} ������� {target.unitData.unitName}");
            return;
        }

        // 4. ���� ���� ����, �� �� � ������� � ��������� � ����, ������ ���� �� ���� �������� ���������
        if (target != null && me.commander != null && me.IsInAura())
        {
            MoveTowardsTarget(me, target);
            Debug.Log($"{me.unitData.unitName} �������� � ����� � ���� ���������");
            return;
        }

        // 5. ���� ������ �� ��������� � �������� ��� (�������� ������)
        if (me.commander != null && !me.IsInAura())
        {
            MoveTowardsTarget(me, me.commander);
            Debug.Log($"{me.unitData.unitName} �������� ������ ���������");
            return;
        }

        // 6. ���� ������ �� ���� � ����� �� �����
        Debug.Log($"{me.unitData.unitName} ����� �� ����� (��� �����/�� � ����)");
    }


    // === ��������������� ������ ===

    private Unit FindBestEnemyTarget(Unit me)
    {
        // ����� ��� ���������� �����
        var allUnits = UnitManager.Instance.AllUnits;
        Unit best = null;
        float minDist = float.MaxValue;
        foreach (var u in allUnits)
        {
            // ���������� ������, ��������� � ������ ����
            if (u == null || u == me || u.currentHP <= 0) continue;

            var rel = FactionManager.Instance.GetRelation(me.faction, u.faction);
            if (rel != FactionManager.RelationType.Enemy) continue;

            float dist = Vector2.Distance(me.transform.position, u.transform.position);
            // **����� ����� ��������: ��� ������ ������ ��������� � ����� ������ �� ������!**
            if (dist < minDist)
            {
                minDist = dist;
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
            return dist == 1 && (dx == 1 ^ dy == 1); // ������ �� ������, �� �� ���������!
        else
            return dist <= range;
    }


    // ��� ��������� � ���� ��������� ������ ����� � ����� �� ������
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
            if (pos.x >= 0 && pos.x < grid.width && pos.y >= 0 && pos.y < grid.height)
            {
                var cell = grid.cells[pos.x, pos.y];
                if (cell.occupyingUnit == null)
                    candidates.Add(cell);
            }
        }

        // ����� ��������� � ����
        Cell best = null;
        float minDist = float.MaxValue;
        foreach (var c in candidates)
        {
            float dist = Vector2.Distance(me.transform.position, c.transform.position);
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
        // ���������� ��� �����
        int attackRange = me.GetAttackRange();

        if (attackRange == 1)
        {
            // ��� ��������� � ���� �������� �� ��� ����, � �������� ������ �� ������
            Cell goal = FindAdjacentFreeCell(me, target);

            if (goal == null)
            {
                Debug.Log($"{me.unitData.unitName}: ��� ��������� ������ � ����!");
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

                        me.MoveTo(cell.transform.position);
                        cell.occupyingUnit = me;
                    }
                    else break;
                }
            }
        }
        else // ��� ��������� � �����
        {
            // ����� ��������� � ��������, ��������� ������� ��������� ��� �������� �������� � ������ ������
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

                        me.MoveTo(cell.transform.position);
                        cell.occupyingUnit = me;
                    }
                    else break;
                }
            }
        }
    }
} 
