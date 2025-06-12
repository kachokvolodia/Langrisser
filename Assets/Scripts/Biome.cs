using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class TerrainTileSet
{
    public TerrainType terrainType;
    public TileBase[] tiles;
}

[CreateAssetMenu(menuName = "Terrain/Biome")]
public class Biome : ScriptableObject
{
    public string biomeName;
    public TerrainTileSet[] terrainTiles;

    public TileBase GetTile(TerrainType type)
    {
        foreach (var set in terrainTiles)
        {
            if (set.terrainType == type && set.tiles != null && set.tiles.Length > 0)
                return set.tiles[Random.Range(0, set.tiles.Length)];
        }
        return null;
    }
}
