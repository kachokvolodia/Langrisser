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
        public bool isBoss;
    }

    public int baseWidth = 28;
    public int baseHeight = 28;
    public int sizeVariation = 4;
    public Biome[] possibleBiomes;

    private List<LevelInfo> levels = new List<LevelInfo>();
    public int CurrentLevel { get; private set; } = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartExpedition();
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

        // choose entry/exit positions on opposite sides
        bool horizontal = Random.value > 0.5f;
        Vector2Int entry;
        Vector2Int exit;
        if (horizontal)
        {
            int yEntry = Random.Range(1, info.height - 2);
            int yExit = Random.Range(1, info.height - 2);
            entry = new Vector2Int(1, yEntry);
            exit = new Vector2Int(info.width - 2, yExit);
        }
        else
        {
            int xEntry = Random.Range(1, info.width - 2);
            int xExit = Random.Range(1, info.width - 2);
            entry = new Vector2Int(xEntry, 1);
            exit = new Vector2Int(xExit, info.height - 2);
        }
        GridManager.Instance.PlaceEntryExit(entry, exit);
    }

    LevelInfo GenerateLevelInfo(int levelIndex)
    {
        var info = new LevelInfo();
        info.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        int variation = Random.Range(-sizeVariation, sizeVariation + 1);
        bool boss = levelIndex % 10 == 0;
        info.isBoss = boss;
        info.width = baseWidth + (boss ? sizeVariation * 2 : variation);
        info.height = baseHeight + (boss ? sizeVariation * 2 : variation);
        if (possibleBiomes != null && possibleBiomes.Length > 0)
        {
            int idx = UnityEngine.Random.Range(0, possibleBiomes.Length);
            info.biome = possibleBiomes[idx];
        }
        return info;
    }
}
