using UnityEngine;
using static Unit;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour
{
    public int moveCost = 1;
    public string terrainType = "Grass";
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color highlightColor = Color.yellow;

    // ��� �������� ��������� ���� ��������
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
        // ������ ��������� (����� ��������)
        spriteRenderer.color = highlightColor;

        // �����: ���� �� ������ ���� ���� � �������� ���� ���������
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
        // ������ ������ �������� �����
        spriteRenderer.color = isMoveHighlight ? Color.cyan : originalColor;

        // �����: �������� ��������� ���� ������!
        UnitManager.Instance.ClearAuraHighlights();
    }

    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return; // ���� ��� �� UI � �� ���������!

        StatusBarUI.Instance.HideCellInfo();

        // ���/����� �� ���������� �����
        if (UnitManager.Instance.HasSelectedUnit())
        {
            if (UnitManager.Instance.CanMoveToCell(this))
            {
                UnitManager.Instance.MoveSelectedUnit(transform.position);
                return;
            }
            else if (UnitManager.Instance.IsAttackHighlightedCell(this))
            {
                UnitManager.Instance.AttackUnitAtCell(this);
                return;
            }
        }

        // ���������� ���� ��� ������ �����
        if (occupyingUnit != null)
        {
            if (occupyingUnit.faction == Unit.Faction.Player && !occupyingUnit.hasActed)
                UnitManager.Instance.SelectUnit(occupyingUnit);

            UnitActionMenu.Instance.ShowMenu(occupyingUnit.transform.position, occupyingUnit);
        }
    }

    // --- ������ ��� ��������� ---
    public void Highlight(Color color)
    {
        spriteRenderer.color = color;
        // ���� ������������ ��� ������� � �������� ��� ������
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
        // ���� ������ ��� ���������� ��� ���� ��������, ��������� cyan,
        // ����� ���������� �������� ����
        if (isMoveHighlight)
            GetComponent<SpriteRenderer>().color = Color.cyan;
        else
            GetComponent<SpriteRenderer>().color = originalColor;
    }
}