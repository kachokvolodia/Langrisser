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
    public Image expBarFill;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI descriptionText;

    public Unit lastUnitForInfo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
        if (expBarFill != null)
        {
            float pct = (float)unit.experience / ExperienceManager.ExpToNextLevel(unit.level);
            expBarFill.fillAmount = Mathf.Clamp01(pct);
        }
        hpText.text = $"HP: {unit.currentHP} / {unit.MaxHP}";
        if (mpText != null)
            mpText.text = $"MP: {unit.currentMP} / {unit.MaxMP}";
        string stats = $"ATK: {unit.Attack}\nDEF: {unit.Defense}\nM.ATK: {unit.MagicAttack}\nM.DEF: {unit.MagicDefense}\nMOV: {unit.MoveRange}\nATK RNG: {unit.GetAttackRange()}";
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
        else if (unit.commander != null)
        {
            var cd = unit.commander.unitData;
            string auraStr = "";
            if (cd.commanderAttackBonus != 0) auraStr += $"ATK +{cd.commanderAttackBonus} ";
            if (cd.commanderDefenseBonus != 0) auraStr += $"DEF +{cd.commanderDefenseBonus} ";
            if (cd.commanderMagicAttackBonus != 0) auraStr += $"M.ATK +{cd.commanderMagicAttackBonus} ";
            if (cd.commanderMagicDefenseBonus != 0) auraStr += $"M.DEF +{cd.commanderMagicDefenseBonus} ";
            if (cd.commanderRangeBonus != 0) auraStr += $"RNG +{cd.commanderRangeBonus}";
            if (!string.IsNullOrEmpty(auraStr))
            {
                if (unit.IsInAura())
                    stats += $"\nВ ауре командира: {auraStr}";
                else
                    stats += $"\nКомандир: {unit.commander.unitData.unitName}";
            }
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
