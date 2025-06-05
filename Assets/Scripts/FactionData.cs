using UnityEngine;

[CreateAssetMenu(fileName = "FactionData", menuName = "SRPG/Faction Data")]
public class FactionData : ScriptableObject
{
    public Unit.Faction faction;
    public Color color = Color.white;
    public Sprite icon;
    public Sprite flag;
    [TextArea]
    public string description;
}
