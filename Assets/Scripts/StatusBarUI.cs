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
    public TextMeshProUGUI weatherInfoText;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowCellInfo(Cell cell)
    {
        ShowTerrainInfo(cell);
        ShowUnitInfo(cell.occupyingUnit);
    }

    void ShowTerrainInfo(Cell cell)
    {
        Vector2Int gridPos = cell.gridPos;
        cellIcon.sprite = GridManager.Instance.terrainTilemap.GetSprite((Vector3Int)gridPos);
        cellIcon.enabled = true;
        cellInfoText.text = $"Местность: {cell.terrainType}\n" +
                            $"Сложн. хода: {cell.moveCost}";
    }

    void ShowUnitInfo(Unit unit)
    {
        if (unit != null)
        {
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

    string FactionToSide(Faction faction)
    {
        var rel = FactionManager.Instance != null
            ? FactionManager.Instance.GetRelation(FactionManager.PlayerFaction, faction)
            : FactionManager.RelationType.Neutral;
        switch (rel)
        {
            case FactionManager.RelationType.Ally: return "Союзник";
            case FactionManager.RelationType.Enemy: return "Враг";
            case FactionManager.RelationType.Neutral: return "Нейтрал";
            default: return "?";
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

    public void SetTurnInfo(Faction faction)
    {
        if (turnInfoText != null)
            turnInfoText.text = $"Ход: {faction}";
    }

    public void SetEndTurnButtonInteractable(bool value)
    {
        if (endTurnButton != null)
            endTurnButton.interactable = value;
    }

    public void SetWeatherInfo(WeatherType weather)
    {
        if (weatherInfoText != null)
            weatherInfoText.text = $"Погода: {weather}";
    }
}

