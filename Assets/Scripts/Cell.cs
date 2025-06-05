using UnityEngine;
using static Unit;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour
{
    public int moveCost = 1;
    public TerrainType terrainType = TerrainType.Grass;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color highlightColor = Color.yellow;

    // Для проверки подсветки зоны движения
    private bool isMoveHighlight = false;

    public Unit occupyingUnit = null;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }
    void OnMouseEnter()
    {
        StatusBarUI.Instance.ShowCellInfo(this);
        // Старая подсветка (можно оставить)
        spriteRenderer.color = highlightColor;

        if (UnitManager.Instance.HasSelectedUnit() && UnitManager.Instance.CanMoveToCell(this))
        {
            UnitManager.Instance.PreviewPath(this);
        }

        // НОВОЕ: если на клетке есть юнит — показать ауру командира
        if (occupyingUnit != null)
        {
            if (occupyingUnit.isCommander)
            {
                UnitManager.Instance.HighlightCommanderAura(occupyingUnit);
            }
            else if (occupyingUnit.commander != null)
            {
                UnitManager.Instance.HighlightCommanderAura(occupyingUnit.commander);
            }
        }
    }

    void OnMouseExit()
    {
        // Старая логика возврата цвета
        spriteRenderer.color = isMoveHighlight ? Color.cyan : originalColor;

        // НОВОЕ: сбросить подсветку ауры всегда!
        UnitManager.Instance.ClearAuraHighlights();
        UnitManager.Instance.HideMovePreview();
    }

    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return; // Клик был по UI — не реагируем!

        StatusBarUI.Instance.HideCellInfo();

        // Ход/атака по выбранному юниту
        if (UnitManager.Instance.HasSelectedUnit())
        {
            if (UnitManager.Instance.CanMoveToCell(this))
            {
                UnitManager.Instance.RequestMoveConfirmation(this);
                return;
            }
            else if (UnitManager.Instance.IsAttackHighlightedCell(this))
            {
                UnitManager.Instance.AttackUnitAtCell(this);
                return;
            }
        }

        // Показываем меню для любого юнита
        if (occupyingUnit != null)
        {
            if (occupyingUnit.faction == FactionManager.PlayerFaction && !occupyingUnit.hasActed)
                UnitManager.Instance.SelectUnit(occupyingUnit);

            UnitActionMenu.Instance.ShowMenu(occupyingUnit.transform.position, occupyingUnit);
        }
    }

    // --- Методы для подсветки ---
    public void Highlight(Color color)
    {
        spriteRenderer.color = color;
        // Если подсвечиваем как ходовую — отмечаем это флагом
        isMoveHighlight = (color == Color.cyan);
    }

    public void Unhighlight()
    {
        spriteRenderer.color = Color.white;
        isMoveHighlight = false;
    }

    public void HighlightAura(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }

    public void UnhighlightAura()
    {
        // Если клетка уже подсвечена как зона движения, оставляем cyan,
        // иначе возвращаем исходный цвет
        if (isMoveHighlight)
            GetComponent<SpriteRenderer>().color = Color.cyan;
        else
            GetComponent<SpriteRenderer>().color = originalColor;
    }

    public bool IsPassable(Unit unit)
    {
        if (terrainType == TerrainType.Ocean || terrainType == TerrainType.Wall || terrainType == TerrainType.River)
        {
            return unit != null && unit.unitData.movementType == MovementType.Flyer;
        }

        if (terrainType == TerrainType.Mountain)
        {
            return unit != null && unit.unitData.movementType == MovementType.Flyer;
        }

        return true;
    }

    public int GetMoveCost(Unit unit)
    {
        if (unit != null && unit.unitData.movementType == MovementType.Flyer)
            return 1;
        switch (terrainType)
        {
            case TerrainType.Forest:
                return unit != null && unit.unitData.movementType == MovementType.Cavalry ? 3 : 2;
            case TerrainType.Mountain:
                return unit != null && unit.unitData.movementType == MovementType.Cavalry ? 4 : 3;
            case TerrainType.Road:
                return 1;
            case TerrainType.Desert:
                return unit != null && unit.unitData.movementType == MovementType.Cavalry ? 3 : 2;
            case TerrainType.Snow:
                return unit != null && unit.unitData.movementType == MovementType.Cavalry ? 3 : 2;
            case TerrainType.Swamp:
                return unit != null && unit.unitData.movementType == MovementType.Cavalry ? 4 : 3;
            case TerrainType.Bridge:
                return 1;
            case TerrainType.Town:
                return 1;
            default:
                return moveCost;
        }
    }
}