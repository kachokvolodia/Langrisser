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
    public TextMeshProUGUI turnInfoText;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowCellInfo(Cell cell)
    {
        // --- Клетка ---
        cellIcon.sprite = cell.GetComponent<SpriteRenderer>().sprite;
        cellIcon.enabled = true;
        cellInfoText.text = $"Местность: {cell.terrainType}\n" +
                            $"Сложн. хода: {cell.moveCost}";
        // --- Юнит ---
        if (cell.occupyingUnit != null)
        {
            var unit = cell.occupyingUnit;
            unitIcon.sprite = unit.GetComponent<SpriteRenderer>().sprite;
            unitIcon.enabled = true;
            unitInfoText.text = $"{unit.unitData.unitName}\n" +
                                $"{unit.unitData.unitClass}\n" +
                                $"Фракция: {unit.faction}\n" +
                                $"Сторона: {FactionToSide(unit.faction)}";
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
            case Unit.Faction.Player: return "Союзник";
            case Unit.Faction.Enemy: return "Враг";
            case Unit.Faction.PlayerAlly: return "Союзник";
            case Unit.Faction.EnemyAlly: return "Враг";
            case Unit.Faction.Neutral: return "Нейтрал";
            case Unit.Faction.EvilNeutral: return "Злой нейтрал";
            default: return "???";
        }
    }

    public void OnEndTurnButtonPressed()
    {
        if (TurnManager.Instance.IsPlayerTurn())
            TurnManager.Instance.EndCurrentTurn();
    }

    public void OnMenuButtonPressed()
    {
        Debug.Log("Меню пока не реализовано!");
        // Тут потом вызовем показ меню-настроек
    }

    public void SetTurnInfo(Unit.Faction faction)
    {
        if (turnInfoText != null)
            turnInfoText.text = $"Ход: {faction}";
    }

    public void SetEndTurnButtonInteractable(bool value)
    {
        if (endTurnButton != null)
            endTurnButton.interactable = value;
    }
}

