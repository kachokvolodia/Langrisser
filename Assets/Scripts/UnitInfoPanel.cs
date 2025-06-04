using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoPanel : MonoBehaviour
{
    public static UnitInfoPanel Instance;

    public GameObject panel;
    public Image portrait;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI classText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI descriptionText;

    public Unit lastUnitForInfo;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowInfo(Unit unit)
    {
        Debug.Log($"[DEBUG] ShowInfo вызван для: {(unit != null ? unit.name : "NULL")}");
        if (unit == null)
        {
            Debug.LogWarning("Нет юнита для инфо!");
            return;
        }

        if (unit == null || unit.unitData == null) return;

        panel.SetActive(true);
        lastUnitForInfo = unit;
        portrait.sprite = unit.GetComponent<SpriteRenderer>().sprite;
        nameText.text = unit.unitData.unitName;
        classText.text = "Класс: " + unit.unitData.unitClass;
        hpText.text = $"HP: {unit.currentHP} / {unit.unitData.maxHP}";
        statsText.text = $"ATK: {unit.unitData.attack}\nDEF: {unit.unitData.defense}\nMOV: {unit.unitData.moveRange}\nATK RNG: {unit.GetAttackRange()}";
        descriptionText.text = unit.unitData.description;
        UnitActionMenu.Instance.HideMenu();
    }

    public void HidePanel()
    {
        panel.SetActive(false);
        // Если это союзник и он может действовать — показываем меню снова
        if (lastUnitForInfo != null && lastUnitForInfo.faction == Unit.Faction.Player && !lastUnitForInfo.hasActed)
        {
            UnitActionMenu.Instance.ShowMenu(lastUnitForInfo.transform.position, lastUnitForInfo);
        }
        lastUnitForInfo = null;
    }
}
