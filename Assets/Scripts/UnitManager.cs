using System.Collections.Generic;
using UnityEngine;
using static Unit;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    public GameObject[] playerCommanderPrefabs;
    public GameObject[] playerSoldierPrefabs;
    public GameObject[] enemyCommanderPrefabs;
    public GameObject[] enemySoldierPrefabs;
    public GameObject[] allyCommanderPrefabs;
    public GameObject[] allySoldierPrefabs;
    public GameObject[] enemyAllyCommanderPrefabs;
    public GameObject[] enemyAllySoldierPrefabs;
    public GameObject[] neutralCommanderPrefabs;
    public GameObject[] neutralSoldierPrefabs;
    public GameObject[] evilNeutralCommanderPrefabs;
    public GameObject[] evilNeutralSoldierPrefabs;


    public GridManager gridManager;
    private Unit selectedUnit;
    private List<Cell> highlightedCells = new List<Cell>();

    // ---- НОВОЕ: AllUnits список ----
    public List<Unit> AllUnits = new List<Unit>();

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AllUnits.Clear();
        int w = gridManager.width;
        int h = gridManager.height;

        // КООРДИНАТЫ для командиров
        Vector2Int playerPos = new Vector2Int(w / 2, h / 2);
        Vector2Int enemyPos = new Vector2Int(w - 2, h - 2);
        Vector2Int allyPos = new Vector2Int(w / 2, h - 2);
        Vector2Int enemyAllyPos = new Vector2Int(w - 2, h / 2);
        Vector2Int neutralPos = new Vector2Int(1, 1);
        Vector2Int evilNeutralPos = new Vector2Int(w - 2, 1);

        // СПАВН: команда игрока
        SpawnSquad(playerCommanderPrefabs, playerSoldierPrefabs, playerPos, 2, Unit.Faction.Player);
        // враг
        SpawnSquad(enemyCommanderPrefabs, enemySoldierPrefabs, enemyPos, 2, Unit.Faction.Enemy);
        // союзник игрока
        SpawnSquad(allyCommanderPrefabs, allySoldierPrefabs, allyPos, 2, Unit.Faction.PlayerAlly);
        // союзник врага
        SpawnSquad(enemyAllyCommanderPrefabs, enemyAllySoldierPrefabs, enemyAllyPos, 2, Unit.Faction.EnemyAlly);
        // нейтралы
        SpawnSquad(neutralCommanderPrefabs, neutralSoldierPrefabs, neutralPos, 2, Unit.Faction.Neutral);
        // Злые Нейтралы
        SpawnSquad(evilNeutralCommanderPrefabs, evilNeutralSoldierPrefabs, evilNeutralPos, 2, Unit.Faction.EvilNeutral);
    }

    // Спавнит командира и n солдат вокруг него по ближайшим свободным клеткам
    public void SpawnSquad(GameObject[] commanderPrefabs, GameObject[] soldierPrefabs, Vector2Int commanderGridPos, int soldierCount, Unit.Faction faction)
    {
        // 1. Выбираем случайный префаб командира
        GameObject commanderPrefab = commanderPrefabs[Random.Range(0, commanderPrefabs.Length)];
        Vector3 commanderWorldPos = gridManager.GetCellCenterPosition(commanderGridPos.x, commanderGridPos.y);

        // 2. Спавним командира
        Unit commander = SpawnUnit(commanderPrefab, commanderWorldPos, faction);
        commander.isCommander = true;
        if (commander.squad == null)
            commander.squad = new List<Unit>();

        // 3. Находим ближайшие клетки
        List<Vector2Int> nearbyCells = GetNearbyCells(commanderGridPos, soldierCount);

        // 4. Спавним солдат, каждому случайный префаб из массива
        for (int i = 0; i < soldierCount; i++)
        {
            Vector2Int cellPos = nearbyCells[i];
            Vector3 worldPos = gridManager.GetCellCenterPosition(cellPos.x, cellPos.y);

            GameObject randomSoldierPrefab = soldierPrefabs[Random.Range(0, soldierPrefabs.Length)];
            Unit soldier = SpawnUnit(randomSoldierPrefab, worldPos, faction);
            soldier.commander = commander;
            commander.squad.Add(soldier);
        }
    }

    // Возвращает soldierCount ближайших к заданной точке клеток (крест + остальные)
    private List<Vector2Int> GetNearbyCells(Vector2Int center, int count)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        int w = gridManager.width;
        int h = gridManager.height;

        // Крест вокруг центра (верх, низ, лево, право)
        Vector2Int[] deltas = new Vector2Int[]
        {
        new Vector2Int(0,1), new Vector2Int(0,-1), new Vector2Int(-1,0), new Vector2Int(1,0),
        new Vector2Int(-1,-1), new Vector2Int(-1,1), new Vector2Int(1,-1), new Vector2Int(1,1),
        new Vector2Int(0,2), new Vector2Int(2,0), new Vector2Int(-2,0), new Vector2Int(0,-2)
        };

        int added = 0;
        int idx = 0;
        while (added < count)
        {
            if (idx == 0) // сначала центр
            {
                if (IsCellFree(center)) { result.Add(center); added++; }
            }
            else if (idx - 1 < deltas.Length)
            {
                Vector2Int pos = center + deltas[idx - 1];
                if (pos.x >= 0 && pos.x < w && pos.y >= 0 && pos.y < h && IsCellFree(pos))
                {
                    result.Add(pos);
                    added++;
                }
            }
            else
            {
                // Просто обходим грид по спирали (если вдруг много солдат)
                for (int r = 2; r < Mathf.Max(w, h); r++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        for (int dy = -r; dy <= r; dy++)
                        {
                            if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;
                            Vector2Int pos = center + new Vector2Int(dx, dy);
                            if (pos.x >= 0 && pos.x < w && pos.y >= 0 && pos.y < h && IsCellFree(pos) && !result.Contains(pos))
                            {
                                result.Add(pos);
                                added++;
                                if (added >= count) break;
                            }
                        }
                        if (added >= count) break;
                    }
                    if (added >= count) break;
                }
            }
            idx++;
        }
        return result;
    }

    // Проверка: клетка свободна?
    private bool IsCellFree(Vector2Int gridPos)
    {
        var cell = gridManager.cells[gridPos.x, gridPos.y];
        return cell.occupyingUnit == null;
    }


    // ---- НОВОЕ: Спавн через менеджер ----
    public Unit SpawnUnit(GameObject prefab, Vector3 position, Faction faction)
    {
        GameObject unitGO = Instantiate(prefab, position, Quaternion.identity);
        Unit unit = unitGO.GetComponent<Unit>();
        unit.faction = faction;

        // Назначаем на клетку
        var cell = GetCellOfUnit(unit);
        if (cell != null)
            cell.occupyingUnit = unit;

        // Регистрация юнита (если вдруг что-то не сработает в Start юнита)
        RegisterUnit(unit);
        return unit;
    }

    // ---- НОВОЕ: Регистрация/удаление ----
    public void RegisterUnit(Unit unit)
    {
        if (!AllUnits.Contains(unit))
            AllUnits.Add(unit);
    }

    public void UnregisterUnit(Unit unit)
    {
        AllUnits.Remove(unit);
    }

    // --- ДАЛЬШЕ идёт твой текущий код (всё по-старому, кроме FindObjectsOfType) ---

    public void SelectUnit(Unit unit)
    {
        Debug.Log($"[DEBUG] SelectUnit called with {unit?.name}, hasActed={unit?.hasActed}");
        if (unit != null && !unit.hasActed)
        {
            if (selectedUnit != null)
                selectedUnit.SetSelected(false);

            selectedUnit = unit;
            unit.SetSelected(true);
            Debug.Log($"[DEBUG] selectedUnit теперь {selectedUnit?.name}");
            UnitActionMenu.Instance.ShowMenu(unit.transform.position, unit);
        }
        else
        {
            Debug.Log("[DEBUG] Не могу выделить юнита: либо он null, либо уже ходил");
            DeselectUnit();
        }
    }

    public void HighlightMovableCells(Unit unit)
    {
        Debug.Log("[DEBUG] Подсвечиваем клетки движения для " + unit.name + " c радиусом " + unit.unitData.moveRange);
        ClearHighlightedCells();
        var startCell = GetCellOfUnit(unit);
        var cellsInRange = PathfindingManager.Instance.GetReachableCells(startCell, unit.unitData.moveRange, unit);
        foreach (var cell in cellsInRange)
        {
            cell.Highlight(Color.cyan);
            highlightedCells.Add(cell);
        }
    }

    public void OnMovePressed()
    {
        Debug.Log("[DEBUG] OnMovePressed, selectedUnit = " + (selectedUnit != null ? selectedUnit.name : "NULL"));
        if (selectedUnit != null)
        {
            HighlightMovableCells(selectedUnit);
        }
        else
        {
            Debug.LogWarning("OnMovePressed: selectedUnit == null!");
        }
    }

    public void OnAttackPressed()
    {
        if (UnitManager.Instance != null && UnitManager.Instance.HasSelectedUnit())
            UnitManager.Instance.HighlightAttackableCells(UnitManager.Instance.selectedUnit);

        UnitActionMenu.Instance.HideMenu();
    }
    public void OnEndTurnPressed()
    {
        if (selectedUnit != null)
        {
            selectedUnit.hasActed = true;
            UnitActionMenu.Instance.HideMenu();
            DeselectUnit();
        }
    }

    public void ClearHighlightedCells()
    {
        foreach (var cell in highlightedCells)
            cell.Unhighlight();
        highlightedCells.Clear();
    }

    public void MoveSelectedUnit(Vector3 targetPosition)
    {
        if (selectedUnit != null)
        {
            // Освободить старую клетку
            var oldCell = GetCellOfUnit(selectedUnit);
            if (oldCell != null)
                oldCell.occupyingUnit = null;

            selectedUnit.MoveTo(targetPosition);
            selectedUnit.hasMoved = true;

            var newCell = GetCellOfUnit(selectedUnit);
            if (newCell != null)
                newCell.occupyingUnit = selectedUnit;

            ClearHighlightedCells();
            UnitActionMenu.Instance.ShowMenu(selectedUnit.transform.position, selectedUnit);
        }
    }

    public bool HasSelectedUnit()
    {
        return selectedUnit != null;
    }

    public bool CanMoveToCell(Cell cell)
    {
        return highlightedCells.Contains(cell);
    }

    private List<Cell> attackHighlightedCells = new List<Cell>();

    public void HighlightAttackableCells(Unit unit)
    {
        ClearAttackHighlightedCells();

        Vector2Int gridPos = GridManager.Instance.WorldToGrid(unit.transform.position);
        var cells = GridManager.Instance.GetCellsInRange(gridPos, 1);

        foreach (var cell in cells)
        {
            Unit u = FindUnitAtCell(cell);
            if (u != null &&
                FactionManager.Instance.GetRelation(unit.faction, u.faction) ==
                    FactionManager.RelationType.Enemy)
            {
                cell.Highlight(Color.red);
                attackHighlightedCells.Add(cell);
            }
        }
    }

    public void ClearAttackHighlightedCells()
    {
        foreach (var cell in attackHighlightedCells)
            cell.Unhighlight();
        attackHighlightedCells.Clear();
    }

    // Поиск юнита, стоящего на заданной клетке
    public Unit FindUnitAtCell(Cell cell)
    {
        Vector2Int cellPos = GridManager.Instance.WorldToGrid(cell.transform.position);
        foreach (var unit in AllUnits)
        {
            Vector2Int unitPos = GridManager.Instance.WorldToGrid(unit.transform.position);
            if (unitPos == cellPos)
                return unit;
        }
        return null;
    }

    public void AttackUnitAtCell(Cell cell)
    {
        Unit target = FindUnitAtCell(cell);
        if (selectedUnit != null && target != null
            && FactionManager.Instance.GetRelation(selectedUnit.faction, target.faction) == FactionManager.RelationType.Enemy)
        {
            ResolveCombat(selectedUnit, target);

            selectedUnit.SetSelected(false);
            selectedUnit = null;
            ClearHighlightedCells();
        }
    }

    public void ResolveCombat(Unit attacker, Unit defender)
    {
        // 1. Считаем урон для обоих, HP до боя!
        int dmgToDefender = attacker.CalculateDamage(defender);
        int dmgToAttacker = 0;

        // 2. Проверка: может ли защитник контратаковать?
        int attackerRange = attacker.GetAttackRange();
        int defenderRange = defender.GetAttackRange();

        // Считаем дистанцию по гриду (чтобы не зависеть от мира)
        Vector2Int attackerPos = GridManager.Instance.WorldToGrid(attacker.transform.position);
        Vector2Int defenderPos = GridManager.Instance.WorldToGrid(defender.transform.position);
        int distance = Mathf.Abs(attackerPos.x - defenderPos.x) + Mathf.Abs(attackerPos.y - defenderPos.y);

        if (defenderRange >= attackerRange && defenderRange >= distance)
        {
            dmgToAttacker = defender.CalculateDamage(attacker);
        }

        // 3. Одновременно применяем урон (оба могут погибнуть)
        defender.TakeDamage(dmgToDefender);
        if (dmgToAttacker > 0) attacker.TakeDamage(dmgToAttacker);

        // 4. Отмечаем как действовавшего (у атакующего)
        attacker.hasAttacked = true;
        attacker.hasActed = true;
    }

    public float GetClassModifier(Unit attacker, Unit defender)
    {
        // Классика: копьё > конница > меч > копьё
        if (attacker.unitData.unitClass == UnitClass.Spearman && defender.unitData.unitClass == UnitClass.Cavalry) return 1.5f;
        if (attacker.unitData.unitClass == UnitClass.Cavalry && defender.unitData.unitClass == UnitClass.Infantry) return 1.5f;
        if (attacker.unitData.unitClass == UnitClass.Infantry && defender.unitData.unitClass == UnitClass.Spearman) return 1.5f;
        // Лучники сильны против летунов
        if (attacker.unitData.unitClass == UnitClass.Archer && defender.unitData.unitClass == UnitClass.Flyer) return 1.5f;
        // В ближнем бою лучники наоборот слабые:
        if (attacker.unitData.unitClass == UnitClass.Infantry
            && defender.unitData.unitClass == UnitClass.Archer
            && attacker.GetAttackRange() == 1)
            return 1.5f;
        // ... и т.д.
        return 1.0f; // обычный случай
    }


    public bool IsAttackHighlightedCell(Cell cell)
    {
        return attackHighlightedCells.Contains(cell);
    }
    public Cell GetCellOfUnit(Unit unit)
    {
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(unit.transform.position);
        return GridManager.Instance.cells[gridPos.x, gridPos.y];
    }
    public void DeselectUnit()
    {
        if (selectedUnit != null)
        {
            selectedUnit.SetSelected(false);
            selectedUnit = null;
        }
    }
    public void ApplyWaitHealing()
    {
        foreach (var unit in AllUnits)
        {
            if (unit.currentHP <= 0) continue;

            // Командир: если не двигался — хил
            if (unit.isCommander && !unit.hasMoved)
            {
                unit.currentHP = Mathf.Min(unit.currentHP + 3, unit.unitData.maxHP);
                // Можно добавить анимацию/эффект
                Debug.Log($"[HEAL] {unit.unitData.unitName} (командир) восстановил 3 HP за ожидание!");
            }
            // Солдат: если рядом с живым командиром — хил
            else if (!unit.isCommander && unit.commander != null && unit.commander.currentHP > 0)
            {
                Vector2Int uPos = GridManager.Instance.WorldToGrid(unit.transform.position);
                Vector2Int cPos = GridManager.Instance.WorldToGrid(unit.commander.transform.position);
                int dx = Mathf.Abs(uPos.x - cPos.x);
                int dy = Mathf.Abs(uPos.y - cPos.y);
                if (dx + dy == 1) // строго по кресту
                {
                    unit.currentHP = Mathf.Min(unit.currentHP + 3, unit.unitData.maxHP);
                    Debug.Log($"[HEAL] {unit.unitData.unitName} (рядом с командиром) восстановил 3 HP!");
                }
            }
        }
    }
    private List<Cell> auraHighlightedCells = new List<Cell>();

    public void HighlightCommanderAura(Unit commander)
    {
        ClearAuraHighlights();
        if (commander == null) return;
        int aura = commander.commanderRadius;
        Vector2Int center = GridManager.Instance.WorldToGrid(commander.transform.position);
        for (int x = 0; x < GridManager.Instance.width; x++)
        {
            for (int y = 0; y < GridManager.Instance.height; y++)
            {
                int dist = Mathf.Abs(center.x - x) + Mathf.Abs(center.y - y);
                if (dist <= aura)
                {
                    var cell = GridManager.Instance.cells[x, y];
                    cell.HighlightAura(new Color(1f, 0.92f, 0.25f, 0.55f)); // светло-желтый/золотой, можешь подобрать цвет
                    auraHighlightedCells.Add(cell);
                }
            }
        }
    }

    public void ClearAuraHighlights()
    {
        foreach (var cell in auraHighlightedCells)
            cell.UnhighlightAura();
        auraHighlightedCells.Clear();
    }

}
