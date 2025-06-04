using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "SRPG/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public int maxHP;
    public int attack;
    public int defense;
    public int moveRange;
    public int attackRange;
    public UnitClass unitClass;
    public MovementType movementType = MovementType.Foot;
    [TextArea]
    public string description;
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

public enum MovementType
{
    Foot,
    Cavalry,
    Flyer
}


