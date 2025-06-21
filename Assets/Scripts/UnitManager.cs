using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    public GameObject[] auroraEmpireCommanderPrefabs;
    public GameObject[] auroraEmpireSoldierPrefabs;
    public GameObject[] goldenHandCommanderPrefabs;
    public GameObject[] goldenHandSoldierPrefabs;
    public GameObject[] moonArchonDominionCommanderPrefabs;
    public GameObject[] moonArchonDominionSoldierPrefabs;
    public GameObject[] neutralCommanderPrefabs;
    public GameObject[] neutralSoldierPrefabs;
    public GameObject[] evilNeutralCommanderPrefabs;
    public GameObject[] evilNeutralSoldierPrefabs;

    public Sprite healthBarSprite;
    public GameObject healthBarPrefab;
    public Vector3 healthBarOffset = new Vector3(0f, -0.4f, 0f);
    public Vector3 healthBarScale = new Vector3(0.8f, 0.1f, 1f);
    public Vector3 healthTextOffset = new Vector3(-0.2f, -0.2f, 0f);
    public float healthTextSize = 3f;


    // Ссылка на GridManager берём через синглтон
    private Unit selectedUnit;
    private List<Cell> highlightedCells = new List<Cell>();

    // Preview line and ghost for move path
    private LineRenderer pathRenderer;
    private GameObject ghostObject;
    private Cell previewCell;
    private Cell pendingMoveCell;
    private bool moveMode = false;

    // ---- НОВОЕ: AllUnits список ----
    public List<Unit> AllUnits = new List<Unit>();

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        pathRenderer = new GameObject("PathPreview").AddComponent<LineRenderer>();
        pathRenderer.positionCount = 0;
        pathRenderer.startWidth = 0.05f;
        pathRenderer.endWidth = 0.05f;
        pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pathRenderer.startColor = Color.yellow;
        pathRenderer.endColor = Color.yellow;

        ghostObject = new GameObject("MoveGhost");
        var sr = ghostObject.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 1f, 1f, 0.5f);
        ghostObject.SetActive(false);
    }

    void Start()
    {
        // Отложим спавн юнитов до момента, когда грид будет полностью инициализирован
    }

    // Вызывается после генерации уровня для размещения начальных отрядов
    public void SpawnInitialUnits()
    {
        AllUnits.Clear();

        Vector2Int playerPos = GetSpawnPointAwayFromBorder(GridManager.Instance.entryPoint);
        Vector2Int evilPos = GetSpawnPointAwayFromBorder(GridManager.Instance.exitPoint);

        SpawnSquad(auroraEmpireCommanderPrefabs, auroraEmpireSoldierPrefabs, playerPos, 2, Faction.AuroraEmpire);
        SpawnSquad(evilNeutralCommanderPrefabs, evilNeutralSoldierPrefabs, evilPos, 2, Faction.EvilNeutral);

        // Проверим, остались ли противники. Если никого нет, открываем выход
        TurnManager.Instance?.CheckVictory();
    }

    // Спавнит командира и n солдат вокруг него по ближайшим свободным клеткам
    public void SpawnSquad(GameObject[] commanderPrefabs, GameObject[] soldierPrefabs, Vector2Int commanderGridPos, int soldierCount, Faction faction)
    {
        // 1. Выбираем случайный префаб командира
        GameObject commanderPrefab = commanderPrefabs[Random.Range(0, commanderPrefabs.Length)];
        Vector3 commanderWorldPos = GridManager.Instance.GetCellCenterPosition(commanderGridPos.x, commanderGridPos.y);

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
            Vector3 worldPos = GridManager.Instance.GetCellCenterPosition(cellPos.x, cellPos.y);

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
        int w = GridManager.Instance.Width;
        int h = GridManager.Instance.Height;

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
        var cell = GridManager.Instance.cells[gridPos.x, gridPos.y];
        if (cell.occupyingUnit != null)
            return false;
        if (!cell.IsPassable(null))
            return false;
        if (gridPos == GridManager.Instance.entryPoint || gridPos == GridManager.Instance.exitPoint)
            return false;
        return true;
    }

    private Vector2Int GetSpawnPointAwayFromBorder(Vector2Int borderCell)
    {
        int w = GridManager.Instance.Width;
        int h = GridManager.Instance.Height;
        Vector2Int result = borderCell;
        if (borderCell.x == 0) result.x = 1;
        else if (borderCell.x == w - 1) result.x = w - 2;
        if (borderCell.y == 0) result.y = 1;
        else if (borderCell.y == h - 1) result.y = h - 2;
        return result;
    }


    // ---- НОВОЕ: Спавн через менеджер ----
    public Unit SpawnUnit(GameObject prefab, Vector3 position, Faction faction)
    {
        GameObject unitGO = Instantiate(prefab, position, Quaternion.identity);
        Unit unit = unitGO.GetComponent<Unit>();
        unit.faction = faction;

        if (healthBarSprite != null)
        {
            GameObject barGO;
            if (healthBarPrefab != null)
            {
                barGO = Instantiate(healthBarPrefab, unitGO.transform);
                barGO.name = "HealthBar";
            }
            else
            {
                barGO = new GameObject("HealthBar");
                barGO.transform.SetParent(unitGO.transform);
            }

            barGO.transform.localPosition = healthBarOffset;
            var hb = barGO.GetComponent<HealthBar>();
            if (hb == null)
                hb = barGO.AddComponent<HealthBar>();
            hb.Initialize(unit, healthBarSprite, GetFactionColor(faction), healthBarScale, healthTextSize, healthTextOffset);
            unit.healthBar = hb;
        }

        // Назначаем на клетку
        var cell = GetCellOfUnit(unit);
        if (cell != null)
            cell.occupyingUnit = unit;

        // Регистрация юнита (если вдруг что-то не сработает в Start юнита)
        RegisterUnit(unit);
        return unit;
    }

    private Color GetFactionColor(Faction faction)
    {
        var data = FactionManager.Instance?.GetFactionData(faction);
        if (data != null)
            return data.color;

        switch (faction)
        {
            case Faction.AuroraEmpire:
                return Color.green;
            case Faction.MoonArchonDominion:
                return Color.red;
            case Faction.GoldenHand:
                return Color.blue;
            default:
                return Color.white;
        }
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

    // Уведомление о смерти юнита: корректируем мораль и убираем из списка
    public void NotifyUnitDied(Unit unit)
    {
        var snapshot = new List<Unit>(AllUnits);
        foreach (var u in snapshot)
        {
            if (u == null || u == unit) continue;
            if (u.faction == unit.faction)
                u.ModifyMorale(-10);
            else if (FactionManager.Instance != null &&
                     FactionManager.Instance.GetRelation(u.faction, unit.faction) == FactionManager.RelationType.Enemy)
                u.ModifyMorale(5);
        }
        UnregisterUnit(unit);
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
            moveMode = true;
            // На всякий случай скрываем клетки атаки
            ClearAttackHighlightedCells();
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
            selectedUnit.SetSelected(false);
            UnitActionMenu.Instance.HideMenu();
            DeselectUnit();
            ClearHighlightedCells();
            ClearAttackHighlightedCells();
        }
    }

    public void ClearHighlightedCells()
    {
        foreach (var cell in highlightedCells)
            cell.Unhighlight();
        highlightedCells.Clear();
        moveMode = false;
        HideMovePreview();
    }

    public void MoveSelectedUnit(Vector3 targetPosition)
    {
        if (selectedUnit != null)
        {
            StartCoroutine(MoveUnitRoutine(selectedUnit, targetPosition));
        }
    }

    IEnumerator MoveUnitRoutine(Unit unit, Vector3 targetPosition)
    {
        var startCell = GetCellOfUnit(unit);
        Cell targetCell = GridManager.Instance.GetCellFromWorld(targetPosition);
        var path = PathfindingManager.Instance.FindPath(startCell, targetCell, unit);
        if (path == null || path.Count == 0)
            yield break;

        // Освободить старую клетку
        if (startCell != null)
            startCell.occupyingUnit = null;

        for (int i = 1; i < path.Count && i <= unit.unitData.moveRange; i++)
        {
            var cell = path[i];
            yield return StartCoroutine(unit.MoveRoutinePublic(cell.worldPos));
        }

        var finalCell = GetCellOfUnit(unit);
        if (finalCell != null)
            finalCell.occupyingUnit = unit;

        if (finalCell != null && GridManager.Instance.IsExitCell(finalCell) && GridManager.Instance.IsExitUnlocked)
        {
            DungeonProgressionManager.Instance.NextLevel();
            yield break;
        }

        unit.hasMoved = true;
        unit.SetSelected(true);

        ClearHighlightedCells();
        UnitActionMenu.Instance.ShowMenu(unit.transform.position, unit);
        HideMovePreview();
        moveMode = false;
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
                FactionManager.Instance != null &&
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
        Vector2Int cellPos = cell.gridPos;
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
            && FactionManager.Instance != null &&
               FactionManager.Instance.GetRelation(selectedUnit.faction, target.faction) == FactionManager.RelationType.Enemy)
        {
            StartCoroutine(ResolveCombat(selectedUnit, target));

            selectedUnit.SetSelected(false);
            selectedUnit = null;
            ClearHighlightedCells();
        }
    }

    public IEnumerator ResolveCombat(Unit attacker, Unit defender)
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

        int defenderHPAfter = defender.currentHP - dmgToDefender;
        int attackerHPAfter = attacker.currentHP - dmgToAttacker;

        if (CombatDisplay.Instance != null)
            yield return StartCoroutine(CombatDisplay.Instance.PlayBattle(attacker, defender, dmgToDefender, dmgToAttacker));

        if (defenderHPAfter <= 0)
        {
            ExperienceManager.AwardExperience(attacker, defender);
            attacker.ModifyMorale(10);
        }
        if (attackerHPAfter <= 0)
        {
            ExperienceManager.AwardExperience(defender, attacker);
            defender.ModifyMorale(10);
        }

        defender.TakeDamage(dmgToDefender);
        if (dmgToAttacker > 0) attacker.TakeDamage(dmgToAttacker);

        // 4. Отмечаем как действовавшего (у атакующего)
        attacker.hasAttacked = true;
        attacker.hasActed = true;
        attacker.SetSelected(false);
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
    public void ApplyWaitHealing(params Faction[] factions)
    {
        HashSet<Faction> allowed;
        if (factions != null && factions.Length > 0)
            allowed = new HashSet<Faction>(factions);
        else
            allowed = null; // null значит все фракции

        foreach (var unit in AllUnits)
        {
            if (allowed != null && !allowed.Contains(unit.faction))
                continue;

            if (unit.currentHP <= 0) continue;

            // Лечение только если юнит вовсе не действовал
            if (unit.hasMoved || unit.hasAttacked || unit.hasActed)
                continue;

            if (unit.isCommander)
            {
                unit.currentHP = Mathf.Min(unit.currentHP + 3, unit.MaxHP);
                Debug.Log($"[HEAL] {unit.unitData.unitName} (командир) восстановил 3 HP за ожидание!");
                unit.UpdateHealthBar();
            }
            else if (unit.commander != null && unit.commander.currentHP > 0)
            {
                Vector2Int uPos = GridManager.Instance.WorldToGrid(unit.transform.position);
                Vector2Int cPos = GridManager.Instance.WorldToGrid(unit.commander.transform.position);
                int dx = Mathf.Abs(uPos.x - cPos.x);
                int dy = Mathf.Abs(uPos.y - cPos.y);
                if (dx + dy == 1)
                {
                    unit.currentHP = Mathf.Min(unit.currentHP + 3, unit.MaxHP);
                    Debug.Log($"[HEAL] {unit.unitData.unitName} (рядом с командиром) восстановил 3 HP!");
                    unit.UpdateHealthBar();
                }
            }

            // Поднять мораль за нахождение в ауре командира
            if (unit.IsInAura())
                unit.ModifyMorale(Mathf.Max(1, unit.GetAuraMoraleBonus()));
        }
    }

    public void ResetUnitsForNextTurn(params Faction[] factions)
    {
        HashSet<Faction> allowed;
        if (factions != null && factions.Length > 0)
            allowed = new HashSet<Faction>(factions);
        else
            allowed = null; // null означает все фракции

        foreach (var unit in AllUnits)
        {
            if (allowed != null && !allowed.Contains(unit.faction))
                continue;

            unit.hasActed = false;
            unit.hasMoved = false;
            unit.hasAttacked = false;
            unit.isSelected = false;
            unit.SetSelected(false);
        }
    }
    private List<Cell> auraHighlightedCells = new List<Cell>();

    public void HighlightCommanderAura(Unit commander)
    {
        ClearAuraHighlights();
        if (commander == null) return;
        int aura = commander.commanderRadius;
        Vector2Int center = GridManager.Instance.WorldToGrid(commander.transform.position);
        for (int x = 0; x < GridManager.Instance.Width; x++)
        {
            for (int y = 0; y < GridManager.Instance.Height; y++)
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

    public bool IsMoveHighlightedCell(Cell cell)
    {
        return highlightedCells.Contains(cell);
    }

    public bool IsAuraHighlightedCell(Cell cell)
    {
        return auraHighlightedCells.Contains(cell);
    }

    // ======== Move preview helpers ========
    public void PreviewPath(Cell cell)
    {
        if (!moveMode || selectedUnit == null || !highlightedCells.Contains(cell))
            return;

        var start = GetCellOfUnit(selectedUnit);
        var path = PathfindingManager.Instance.FindPath(start, cell, selectedUnit);
        if (path == null || path.Count == 0)
            return;

        pathRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            pathRenderer.SetPosition(i, path[i].worldPos + Vector3.forward * -0.1f);

        ghostObject.GetComponent<SpriteRenderer>().sprite = selectedUnit.GetComponent<SpriteRenderer>().sprite;
        ghostObject.transform.position = cell.worldPos;
        ghostObject.SetActive(true);
        previewCell = cell;
    }

    public void HideMovePreview()
    {
        pathRenderer.positionCount = 0;
        ghostObject.SetActive(false);
        previewCell = null;
    }

    public void RequestMoveConfirmation(Cell cell)
    {
        pendingMoveCell = cell;
        MoveConfirmPanel.Instance?.Show(cell);
    }

    public void ConfirmMove(Cell cell)
    {
        MoveSelectedUnit(cell.worldPos);
        pendingMoveCell = null;
        moveMode = false;
        HideMovePreview();
    }

    public void CancelMove()
    {
        pendingMoveCell = null;
        HideMovePreview();
    }

}
