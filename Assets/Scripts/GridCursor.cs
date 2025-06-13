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
            }
            lastCell = cell;
        }
    }
}
