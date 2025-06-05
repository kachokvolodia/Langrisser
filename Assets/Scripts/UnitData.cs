using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "SRPG/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public int maxHP;
    public int maxMP;
    public int attack;
    public int magicAttack;
    public int defense;
    public int magicDefense;
    public int moveRange;
    public int attackRange;
    public UnitClass unitClass;
    public MovementType movementType = MovementType.Foot;
    [Header("Commander Bonuses")]
    public int commanderAttackBonus;
    public int commanderDefenseBonus;
    public int commanderMagicAttackBonus;
    public int commanderMagicDefenseBonus;
    public int commanderRangeBonus;
    [TextArea]
    public string description;

    [System.Serializable]
    public class FactionVariant
    {
        public Unit.Faction faction;
        public string unitName;
        public Sprite sprite;
    }

    public FactionVariant[] factionVariants;
}
public enum UnitClass
{
    Infantry,
    Cavalry,
    Spearman,
    Archer,
    Flyer,
    Mage,
    // и т.д.
}


