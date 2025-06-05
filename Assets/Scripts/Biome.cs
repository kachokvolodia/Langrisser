using UnityEngine;

[System.Serializable]
public class TerrainSpriteSet
{
    public TerrainType terrainType;
    public Sprite[] sprites;
}

[CreateAssetMenu(menuName = "Terrain/Biome")]
public class Biome : ScriptableObject
{
    public string biomeName;
    public TerrainSpriteSet[] terrainSprites;

    public Sprite GetSprite(TerrainType type)
    {
        foreach (var set in terrainSprites)
        {
            if (set.terrainType == type && set.sprites != null && set.sprites.Length > 0)
            {
                return set.sprites[Random.Range(0, set.sprites.Length)];
            }
        }
        return null;
    }
}
