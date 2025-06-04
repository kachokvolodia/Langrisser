using UnityEngine;
using UnityEngine.UI;

public class MoveConfirmPanel : MonoBehaviour
{
    public static MoveConfirmPanel Instance;

    public GameObject panel;
    public Button confirmButton;
    public Button cancelButton;

    private Cell targetCell;

    private void Awake()
    {
        Instance = this;
        if (panel != null)
            panel.SetActive(false);
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);
    }

    public void Show(Cell cell)
    {
        targetCell = cell;
        if (panel == null) return;
        panel.SetActive(true);
        panel.transform.position = Camera.main.WorldToScreenPoint(cell.transform.position);
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
        targetCell = null;
    }

    void OnConfirm()
    {
        if (targetCell != null)
            UnitManager.Instance.ConfirmMove(targetCell);
        Hide();
    }

    void OnCancel()
    {
        UnitManager.Instance.CancelMove();
        Hide();
    }
}
