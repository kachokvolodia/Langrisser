using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool hasMoved = false;
    public bool hasAttacked = false;
    public bool hasActed = false;

    // ===== Langrisser-���������� =====
    public bool isCommander = false;      // ���� ���� � ��������?
    public int commanderRadius = 2;       // ������ ���� ��������� (���� ��� ��������)
    public Unit commander;                // ��� �������� ����� � ������ �� ���������
    public List<Unit> squad;              // ��� ��������� � ������ ������

    // ��������: � ���� ���������?
    public bool IsInAura()
    {
        if (isCommander) return true; // �������� ������ � ����� ���� :)
        if (commander == null) return false;
        // ������� ���������� �� �����
        Vector2Int myGrid = GridManager.Instance.WorldToGrid(transform.position);
        Vector2Int comGrid = GridManager.Instance.WorldToGrid(commander.transform.position);
        int dist = Mathf.Abs(myGrid.x - comGrid.x) + Mathf.Abs(myGrid.y - comGrid.y);
        return dist <= commander.commanderRadius;
    }

    public enum Faction
    {
        Player,
        PlayerAlly,    // ������� ������
        Enemy,
        EnemyAlly,     // ������� �����
        Neutral,        // ��������
        EvilNeutral
    }

    public UnitData unitData;
    public int currentHP;
    public Faction faction;

    public bool isSelected = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        if (unitData != null)
            currentHP = unitData.maxHP;
        else
            Debug.LogWarning("UnitData �� �������� �� " + gameObject.name);
    }

    // --- �����: ��������������� ---
    void Start()
    {
        if (isCommander && squad == null)
            squad = new List<Unit>();

        if (UnitManager.Instance != null)
            UnitManager.Instance.RegisterUnit(this);
    }

    void OnDestroy()
    {
        if (UnitManager.Instance != null)
            UnitManager.Instance.UnregisterUnit(this);
    }

    public void OnMovePressed()
    {
        if (UnitManager.Instance != null && UnitManager.Instance.HasSelectedUnit())
            UnitManager.Instance.HighlightMovableCells(this);

        UnitActionMenu.Instance.HideMenu();
    }

    public void SetSelected(bool selected)
    {
        if (TurnManager.Instance != null && TurnManager.Instance.IsPlayerTurn())
        {
            isSelected = selected;
            spriteRenderer.color = selected ? Color.cyan : originalColor;

            if (hasActed)
                spriteRenderer.color = Color.gray;
            else
                spriteRenderer.color = selected ? Color.cyan : originalColor;
        }
    }

    public void MoveTo(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    public void TakeDamage(int amount)
    {
        Debug.Log($"{name} ������� ����: {amount}, HP ����: {currentHP}");
        currentHP -= amount;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
        if (TurnManager.Instance != null)
            TurnManager.Instance.CheckVictory();
    }


    public int CalculateDamage(Unit target)
    {
        float myPower = unitData.attack * ((float)currentHP / unitData.maxHP);
        float theirDef = target.unitData.defense * ((float)target.currentHP / target.unitData.maxHP);

        // ======= ������ �� ���� =======
        if (IsInAura()) myPower += 2;             // ���� � ����, ����� +2
        if (target.IsInAura()) theirDef += 1;     // ���� ���� � ����, ������ +1

        float modifier = UnitManager.Instance.GetClassModifier(this, target);
        int dmg = Mathf.Max(1, Mathf.RoundToInt((myPower - theirDef) * modifier));
        return dmg;
    }




    public int GetMoveRange() => unitData != null ? unitData.moveRange : 1;
    public int GetAttackRange() => unitData != null ? unitData.attackRange : 1;

    void OnMouseEnter()
    {
        // ���������� ���� ������������ �� ��������� �� ��������� ��� �� ��� �������
        if (isCommander)
        {
            UnitManager.Instance.HighlightCommanderAura(this);
        }
        else if (commander != null)
        {
            UnitManager.Instance.HighlightCommanderAura(commander);
        }
    }

    void OnMouseExit()
    {
        UnitManager.Instance.ClearAuraHighlights();
    }

}
