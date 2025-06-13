using UnityEngine;
using UnityEngine.EventSystems;

public class GridCursor : MonoBehaviour
{
    public Color hoverColor = new Color(1f, 1f, 0.5f, 0.5f);
    private Cell lastCell;

    void Update()
    {
        if (GridManager.Instance == null) return;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Cell cell = GridManager.Instance.GetCellFromWorld(mouseWorld);
        if (cell != lastCell)
        {
            if (lastCell != null)
            {
                bool restoreMove = UnitManager.Instance != null && UnitManager.Instance.IsMoveHighlightedCell(lastCell);
                bool restoreAttack = UnitManager.Instance != null && UnitManager.Instance.IsAttackHighlightedCell(lastCell);
                bool restoreAura = UnitManager.Instance != null && UnitManager.Instance.IsAuraHighlightedCell(lastCell);

                lastCell.Unhighlight();
                if (restoreMove && !restoreAura)
                    lastCell.Highlight(Color.cyan);
                if (restoreAttack && !restoreAura && !restoreMove)
                    lastCell.Highlight(Color.red);
                if (restoreAura)
                    lastCell.HighlightAura(new Color(1f, 0.92f, 0.25f, 0.55f));

                StatusBarUI.Instance?.HideCellInfo();
            }
            if (cell != null)
            {
                cell.Highlight(hoverColor);
                StatusBarUI.Instance?.ShowCellInfo(cell);
                UnitManager.Instance?.PreviewPath(cell);
            }
            lastCell = cell;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (UnitManager.Instance != null)
            {
                if (cell != null && UnitManager.Instance.HasSelectedUnit() && UnitManager.Instance.CanMoveToCell(cell))
                {
                    UnitManager.Instance.RequestMoveConfirmation(cell);
                }
                else if (cell != null && cell.occupyingUnit != null)
                {
                    UnitManager.Instance.SelectUnit(cell.occupyingUnit);
                }
                else
                {
                    UnitManager.Instance.DeselectUnit();
                    UnitActionMenu.Instance?.HideMenu();
                }
            }
        }
    }
}