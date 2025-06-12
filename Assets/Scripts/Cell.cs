using UnityEngine;

public class Cell
{
    public int moveCost = 1;
    public TerrainType terrainType = TerrainType.Grass;
    public Unit occupyingUnit = null;
    public Vector2Int gridPos;
    public Vector3 worldPos;

    private bool isMoveHighlight = false;

    public void Highlight(Color color)
    {
        GridManager.Instance.SetTileColor(gridPos, color);
        isMoveHighlight = (color == Color.cyan);
    }

    public void Unhighlight()
    {
        GridManager.Instance.SetTileColor(gridPos, Color.white);
        isMoveHighlight = false;
    }

    public void HighlightAura(Color color)
    {
        GridManager.Instance.SetTileColor(gridPos, color);
    }

    public void UnhighlightAura()
    {
        if (isMoveHighlight)
            GridManager.Instance.SetTileColor(gridPos, Color.cyan);
        else
            GridManager.Instance.SetTileColor(gridPos, Color.white);
    }

    public bool IsPassable(Unit unit)
    {
        if (terrainType == TerrainType.Ocean || terrainType == TerrainType.Wall || terrainType == TerrainType.River || terrainType == TerrainType.Cliff)
            return unit != null && unit.unitData.movementType == MovementType.Flyer;

        if (terrainType == TerrainType.Mountain)
            return unit != null && unit.unitData.movementType == MovementType.Flyer;

        return true;
    }

    public int GetMoveCost(Unit unit)
    {
        if (unit != null && unit.unitData.movementType == MovementType.Flyer)
            return 1;
        switch (terrainType)
        {
            case TerrainType.Forest:
                return unit != null && unit.unitData.movementType == MovementType.Cavalry ? 3 : 2;
            case TerrainType.Mountain:
                return unit != null && unit.unitData.movementType == MovementType.Cavalry ? 4 : 3;
            case TerrainType.Road:
                return 1;
            case TerrainType.Desert:
                return unit != null && unit.unitData.movementType == MovementType.Cavalry ? 3 : 2;
            case TerrainType.Snow:
                return unit != null && unit.unitData.movementType == MovementType.Cavalry ? 3 : 2;
            case TerrainType.Swamp:
                return unit != null && unit.unitData.movementType == MovementType.Cavalry ? 4 : 3;
            case TerrainType.Bridge:
                return 1;
            case TerrainType.Town:
                return 1;
            case TerrainType.Ladder:
                return 2;
            case TerrainType.Gate:
                return 1;
            case TerrainType.River:
            case TerrainType.Cliff:
            case TerrainType.Wall:
                return 99;
            default:
                return moveCost;
        }
    }

    public void SetBaseColor(Color color)
    {
        GridManager.Instance.SetTileColor(gridPos, color);
    }
}
