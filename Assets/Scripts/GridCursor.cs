using UnityEngine;

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
                lastCell.Unhighlight();
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
