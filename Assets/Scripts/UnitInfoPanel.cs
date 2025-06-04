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
    public TextMeshProUGUI levelText;
    public Image expBar;
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
        levelText.text = "LVL: " + unit.level;
        if (expBar != null)
        {
            float pct = (float)unit.experience / ExperienceManager.ExpToNextLevel(unit.level);
            expBar.rectTransform.localScale = new Vector3(Mathf.Clamp01(pct), 1f, 1f);
        }
        hpText.text = $"HP: {unit.currentHP} / {unit.MaxHP}";
        string stats = $"ATK: {unit.Attack}\nDEF: {unit.Defense}\nM.ATK: {unit.MagicAttack}\nM.DEF: {unit.MagicDefense}\nMP: {unit.currentMP}/{unit.MaxMP}\nMOV: {unit.MoveRange}\nATK RNG: {unit.GetAttackRange()}";
        if (unit.isCommander)
        {
            string commanderStr = "";
            if (unit.unitData.commanderAttackBonus != 0) commanderStr += $"ATK +{unit.unitData.commanderAttackBonus} ";
            if (unit.unitData.commanderDefenseBonus != 0) commanderStr += $"DEF +{unit.unitData.commanderDefenseBonus} ";
            if (unit.unitData.commanderMagicAttackBonus != 0) commanderStr += $"M.ATK +{unit.unitData.commanderMagicAttackBonus} ";
            if (unit.unitData.commanderMagicDefenseBonus != 0) commanderStr += $"M.DEF +{unit.unitData.commanderMagicDefenseBonus} ";
            if (unit.unitData.commanderRangeBonus != 0) commanderStr += $"RNG +{unit.unitData.commanderRangeBonus}";
            if (!string.IsNullOrEmpty(commanderStr))
                stats += $"\nКомандование: {commanderStr}";
        }
        statsText.text = stats;
        descriptionText.text = unit.unitData.description;
    }

    public void HidePanel()
    {
        panel.SetActive(false);
        lastUnitForInfo = null;
    }
}
