using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool hasMoved = false;
    public bool hasAttacked = false;
    public bool hasActed = false;

    // ===== Langrisser-командирка =====
    public bool isCommander = false;      // Этот юнит — командир?
    public int commanderRadius = 2;       // Радиус ауры командира (если сам командир)
    public Unit commander;                // Для обычного юнита — ссылка на командира
    public List<Unit> squad;              // Для командира — список солдат

    // Проверка: в ауре командира?
    public bool IsInAura()
    {
        if (isCommander) return true; // Командир всегда в своей ауре :)
        if (commander == null) return false;
        // Считаем расстояние по гриду
        Vector2Int myGrid = GridManager.Instance.WorldToGrid(transform.position);
        Vector2Int comGrid = GridManager.Instance.WorldToGrid(commander.transform.position);
        int dist = Mathf.Abs(myGrid.x - comGrid.x) + Mathf.Abs(myGrid.y - comGrid.y);
        return dist <= commander.commanderRadius;
    }

    public enum Faction
    {
        AuroraEmpire,
        MoonArchonDominion,
        GoldenHand,
        Neutral,        // нейтралы
        EvilNeutral
    }

    public UnitData unitData;
    public int currentHP;
    public int currentMP;
    public int level = 1;
    public int experience = 0;
    // --- Новое: показатель морали (0-100) ---
    public int morale = 50;
    public Faction faction;

    // Дополнительные бонусы от повышения уровня
    public int bonusAttack = 0;
    public int bonusDefense = 0;
    public int bonusMagicAttack = 0;
    public int bonusMagicDefense = 0;
    public int bonusMaxHP = 0;
    public int bonusMaxMP = 0;

    public int MaxHP => unitData.maxHP + bonusMaxHP;
    public int MaxMP => unitData.maxMP + bonusMaxMP;
    public int Attack => unitData.attack + bonusAttack;
    public int Defense => unitData.defense + bonusDefense;
    public int MagicAttack => unitData.magicAttack + bonusMagicAttack;
    public int MagicDefense => unitData.magicDefense + bonusMagicDefense;
    public int MoveRange => unitData.moveRange;
    public int AttackRangeBase => unitData.attackRange;
    public int attackRangeBonus = 0;
    public int AttackRangeTotal => AttackRangeBase + attackRangeBonus;

    // ----- Commander aura bonuses -----
    public int GetAuraAttackBonus()
    {
        if (!IsInAura()) return 0;
        Unit src = isCommander ? this : commander;
        return src != null ? src.unitData.commanderAttackBonus : 0;
    }

    public int GetAuraDefenseBonus()
    {
        if (!IsInAura()) return 0;
        Unit src = isCommander ? this : commander;
        return src != null ? src.unitData.commanderDefenseBonus : 0;
    }

    public int GetAuraMagicAttackBonus()
    {
        if (!IsInAura()) return 0;
        Unit src = isCommander ? this : commander;
        return src != null ? src.unitData.commanderMagicAttackBonus : 0;
    }

    public int GetAuraMagicDefenseBonus()
    {
        if (!IsInAura()) return 0;
        Unit src = isCommander ? this : commander;
        return src != null ? src.unitData.commanderMagicDefenseBonus : 0;
    }

    public int GetAuraRangeBonus()
    {
        if (!IsInAura()) return 0;
        Unit src = isCommander ? this : commander;
        return src != null ? src.unitData.commanderRangeBonus : 0;
    }

    public int GetAuraMoraleBonus()
    {
        if (!IsInAura()) return 0;
        Unit src = isCommander ? this : commander;
        return src != null ? src.unitData.commanderMoraleBonus : 0;
    }

    [HideInInspector]
    public HealthBar healthBar;

    public bool isSelected = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine moveCoroutine;
    public float moveSpeed = 2f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        if (unitData != null)
        {
            currentHP = MaxHP;
            currentMP = MaxMP;
        }
        else
            Debug.LogWarning("UnitData не назначен на " + gameObject.name);
    }

    // --- НОВОЕ: саморегистрация ---
    void Start()
    {
        if (isCommander && squad == null)
            squad = new List<Unit>();

        if (UnitManager.Instance != null)
            UnitManager.Instance.RegisterUnit(this);

        UpdateHealthBar();
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
        isSelected = selected;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (isSelected)
            spriteRenderer.color = Color.cyan;
        else if (hasActed)
            spriteRenderer.color = Color.gray;
        else
            spriteRenderer.color = originalColor;
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutineInternal(targetPosition));
    }

    IEnumerator MoveRoutineInternal(Vector3 target)
    {
        CameraController.Instance?.Follow(transform);
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        CameraController.Instance?.ClearFollow(transform);
        moveCoroutine = null;
    }

    public IEnumerator MoveRoutinePublic(Vector3 target)
    {
        yield return MoveRoutineInternal(target);
    }

    public void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.UpdateBar();
    }

    public void AddExperience(int amount)
    {
        if (level >= ExperienceManager.MaxLevel) return;
        experience += amount;
        while (level < ExperienceManager.MaxLevel &&
               experience >= ExperienceManager.ExpToNextLevel(level))
        {
            experience -= ExperienceManager.ExpToNextLevel(level);
            LevelUp();
        }
    }

    // Изменить мораль юнита на указанное значение
    public void ModifyMorale(int amount)
    {
        morale = Mathf.Clamp(morale + amount, 0, 100);
    }

    void LevelUp()
    {
        level++;
        bonusAttack += 1;
        bonusDefense += 1;
        bonusMagicAttack += 1;
        bonusMagicDefense += 1;
        bonusMaxHP += 2;
        bonusMaxMP += 2;
        currentHP = Mathf.Min(currentHP + 2, MaxHP);
        currentMP = Mathf.Min(currentMP + 2, MaxMP);
        Debug.Log($"{name} повысил уровень до {level}!");
    }

    public void TakeDamage(int amount)
    {
        Debug.Log($"{name} получил урон: {amount}, HP было: {currentHP}");
        currentHP -= amount;
        UpdateHealthBar();
        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        // Сообщаем менеджеру о смерти юнита (он снимет с учёта и скорректирует мораль)
        if (UnitManager.Instance != null)
            UnitManager.Instance.NotifyUnitDied(this);

        if (healthBar != null)
            Destroy(healthBar.gameObject);

        // Если этот юнит — командир, его солдаты теряют связь и мораль
        if (isCommander && squad != null)
        {
            var soldiers = new List<Unit>(squad);
            foreach (var s in soldiers)
            {
                if (s != null)
                {
                    s.ModifyMorale(-30);
                    s.commander = null;
                }
            }
            squad.Clear();
        }

        // Clear the cell this unit occupies
        var cell = UnitManager.Instance.GetCellOfUnit(this);
        if (cell != null && cell.occupyingUnit == this)
            cell.occupyingUnit = null;

        // Remove from commander's squad if applicable
        if (commander != null && commander.squad != null)
            commander.squad.Remove(this);

        Destroy(gameObject);
        if (TurnManager.Instance != null)
            TurnManager.Instance.CheckVictory();
    }


    public int CalculateDamage(Unit target)
    {
        float moraleAtk = 1f + (morale - 50f) / 250f;
        float moraleDef = 1f + (target.morale - 50f) / 250f;

        float myPower = Attack * moraleAtk * ((float)currentHP / MaxHP);
        float theirDef = target.Defense * moraleDef * ((float)target.currentHP / target.MaxHP);

        myPower += GetAttackBonus();
        theirDef += target.GetDefenseBonus();

        // ======= Бонусы за ауру командира =======
        myPower += GetAuraAttackBonus();
        theirDef += target.GetAuraDefenseBonus();

        // Старые плоские бонусы ауры
        if (IsInAura()) myPower += 2;             // если в ауре, атака +2
        if (target.IsInAura()) theirDef += 1;     // если цель в ауре, защита +1

        float modifier = UnitManager.Instance.GetClassModifier(this, target);
        int dmg = Mathf.Max(1, Mathf.RoundToInt((myPower - theirDef) * modifier));
        return dmg;
    }




    public int GetMoveRange()
    {
        int range = unitData != null ? MoveRange : 1;
        if (WeatherManager.Instance != null && WeatherManager.Instance.CurrentWeather == WeatherType.Rain)
            range = Mathf.Max(1, range - 1);
        return range;
    }

    public int GetAttackRange()
    {
        int range = unitData != null ? AttackRangeTotal : 1;
        if (WeatherManager.Instance != null && WeatherManager.Instance.CurrentWeather == WeatherType.Fog)
            range = Mathf.Max(1, range - 1);
        Cell cell = UnitManager.Instance != null ? UnitManager.Instance.GetCellOfUnit(this) : null;
        if (unitData != null && unitData.unitClass == UnitClass.Archer && cell != null &&
            (cell.terrainType == TerrainType.Hill || cell.terrainType == TerrainType.Mountain))
        {
            range += 1; // бонус за высоту
        }
        range += GetAuraRangeBonus();
        return range;
    }

    public int GetDefenseBonus()
    {
        Cell cell = UnitManager.Instance != null ? UnitManager.Instance.GetCellOfUnit(this) : null;
        if (cell == null) return 0;
        if (unitData.unitClass == UnitClass.Spearman && cell.terrainType == TerrainType.Wall)
            return 2;
        if (cell.terrainType == TerrainType.Forest && unitData.movementType != MovementType.Flyer)
            return 1;
        if (cell.terrainType == TerrainType.Town)
            return 1;
        return 0;
    }

    public int GetAttackBonus()
    {
        Cell cell = UnitManager.Instance != null ? UnitManager.Instance.GetCellOfUnit(this) : null;
        if (cell == null) return 0;
        if (unitData.unitClass == UnitClass.Cavalry && (cell.terrainType == TerrainType.Road || cell.terrainType == TerrainType.Grass))
            return 1;
        if (unitData.unitClass == UnitClass.Cavalry && cell.terrainType == TerrainType.Forest)
            return -1;
        if (unitData.unitClass == UnitClass.Cavalry && cell.terrainType == TerrainType.Bridge)
            return 1;
        if (unitData.unitClass == UnitClass.Cavalry && (cell.terrainType == TerrainType.Desert || cell.terrainType == TerrainType.Snow))
            return -1;
        if (cell.terrainType == TerrainType.Town)
            return 1;
        return 0;
    }

    void OnMouseEnter()
    {
        // Показываем ауру командования по наведению на командира или на его солдата
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
