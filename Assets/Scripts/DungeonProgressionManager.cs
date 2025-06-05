using System.Collections.Generic;
using UnityEngine;

public class DungeonProgressionManager : MonoBehaviour
{
    public static DungeonProgressionManager Instance { get; private set; }

    [System.Serializable]
    public class LevelInfo
    {
        public int seed;
        public int width;
        public int height;
        public Biome biome;
    }

    public int baseWidth = 10;
    public int baseHeight = 10;
    public int difficultyStep = 2;
    public Biome[] possibleBiomes;

    private List<LevelInfo> levels = new List<LevelInfo>();
    public int CurrentLevel { get; private set; } = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void StartExpedition()
    {
        CurrentLevel = 1;
        EnsureLevel(CurrentLevel);
    }

    public void NextLevel()
    {
        CurrentLevel++;
        EnsureLevel(CurrentLevel);
    }

    void EnsureLevel(int level)
    {
        while (levels.Count < level)
        {
            levels.Add(GenerateLevelInfo(levels.Count + 1));
        }
        var info = levels[level - 1];
        GridManager.Instance.Initialize(info.width, info.height, info.seed, info.biome);
    }

    LevelInfo GenerateLevelInfo(int levelIndex)
    {
        var info = new LevelInfo();
        info.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        info.width = baseWidth + difficultyStep * (levelIndex - 1);
        info.height = baseHeight + difficultyStep * (levelIndex - 1);
        if (possibleBiomes != null && possibleBiomes.Length > 0)
        {
            int idx = UnityEngine.Random.Range(0, possibleBiomes.Length);
            info.biome = possibleBiomes[idx];
        }
        return info;
    }
}
