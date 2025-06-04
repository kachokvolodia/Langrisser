using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusBarUI : MonoBehaviour
{
    public static StatusBarUI Instance;

    public Image cellIcon;
    public TextMeshProUGUI cellInfoText;
    public Image unitIcon;
    public TextMeshProUGUI unitInfoText;

    public Button endTurnButton;
    public Button menuButton;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowCellInfo(Cell cell)
    {
        // --- ������ ---
        cellIcon.sprite = cell.GetComponent<SpriteRenderer>().sprite;
        cellIcon.enabled = true;
        cellInfoText.text = $"���������: {cell.terrainType}\n" +
                            $"�����. ����: {cell.moveCost}";
        // --- ���� ---
        if (cell.occupyingUnit != null)
        {
            var unit = cell.occupyingUnit;
            unitIcon.sprite = unit.GetComponent<SpriteRenderer>().sprite;
            unitIcon.enabled = true;
            unitInfoText.text = $"{unit.unitData.unitName}\n" +
                                $"{unit.unitData.unitClass}\n" +
                                $"�������: {unit.faction}\n" +
                                $"�������: {FactionToSide(unit.faction)}";
        }
        else
        {
            unitIcon.enabled = false;
            unitInfoText.text = "";
        }
    }

    public void HideCellInfo()
    {
        cellIcon.enabled = false;
        cellInfoText.text = "";
        unitIcon.enabled = false;
        unitInfoText.text = "";
    }

    string FactionToSide(Unit.Faction faction)
    {
        switch (faction)
        {
            case Unit.Faction.Player: return "�������";
            case Unit.Faction.Enemy: return "����";
            case Unit.Faction.PlayerAlly: return "�������";
            case Unit.Faction.EnemyAlly: return "����";
            case Unit.Faction.Neutral: return "�������";
            case Unit.Faction.EvilNeutral: return "���� �������";
            default: return "???";
        }
    }

    public void OnEndTurnButtonPressed()
    {
        TurnManager.Instance.EndPlayerTurn();
    }

    public void OnMenuButtonPressed()
    {
        Debug.Log("���� ���� �� �����������!");
        // ��� ����� ������� ����� ����-��������
    }
}
