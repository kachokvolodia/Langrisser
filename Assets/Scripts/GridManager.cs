using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public Sprite[] tileSprites;
    public Biome biome;
    public Material cellMaterial;

    [Header("Noise Settings")]
    public bool usePerlinNoise = false;
    public float noiseScale = 0.1f;

    // Seed for deterministic generation
    public int seed = 0;
    public bool randomSeed = true;

    public Cell[,] cells;

    private System.Random rng;

    public Color entryHighlight = new Color(0.8f, 1f, 0.6f);
    public Color exitHighlight = new Color(1f, 0.6f, 0.6f);

    public Vector2Int entryPoint;
    public Vector2Int exitPoint;
    private bool exitUnlocked = false;

    private void Awake()
    {
        Instance = this;
        if (randomSeed)
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }

    public void Initialize(int w, int h, int newSeed, Biome newBiome = null)
    {
        width = w;
        height = h;
        seed = newSeed;
        if (newBiome != null)
            biome = newBiome;

        ClearGrid();
        GenerateGrid();
    }

    void ClearGrid()
    {
        if (cells == null) return;
        foreach (var c in cells)
        {
            if (c != null)
                Destroy(c.gameObject);
        }
    }
    void Start()
    {
    }
    Sprite GetSpriteForType(TerrainType type)
    {
        if (biome != null)
        {
            Sprite s = biome.GetSprite(type);
            if (s != null) return s;
        }
        if (tileSprites != null && tileSprites.Length > 0)
            return tileSprites[rng.Next(tileSprites.Length)];
        return null;
    }

    int GetMoveCostForType(TerrainType tType)
    {
        switch (tType)
        {
            case TerrainType.Forest:
            case TerrainType.Hill:
            case TerrainType.Desert:
            case TerrainType.Snow:
                return 2;
            case TerrainType.Mountain:
            case TerrainType.Swamp:
                return 3;
            case TerrainType.Ladder:
                return 2;
            case TerrainType.Ocean:
            case TerrainType.Road:
            case TerrainType.Bridge:
            case TerrainType.Town:
                return 1;
            case TerrainType.River:
                return 99;
            case TerrainType.Gate:
                return 1;
            case TerrainType.Wall:
            case TerrainType.Cliff:
                return 99;
            default:
                return 1;
        }
    }

    void SetCellTerrain(int x, int y, TerrainType tType)
    {
        Cell cell = cells[x, y];
        cell.terrainType = tType;
        cell.moveCost = GetMoveCostForType(tType);
        cell.GetComponent<SpriteRenderer>().sprite = GetSpriteForType(tType);
    }

    TerrainType ChooseTerrainType(int x, int y)
    {
        float noise = Mathf.PerlinNoise((x + seed) * noiseScale, (y + seed) * noiseScale);
        if (noise < 0.2f) return TerrainType.Ocean;
        if (noise < 0.4f) return TerrainType.Forest;
        if (noise < 0.6f) return TerrainType.Grass;
        if (noise < 0.8f) return TerrainType.Hill;
        return TerrainType.Mountain;
    }

    TerrainType ChooseTerrainType()
    {
        if (biome != null && biome.terrainSprites != null && biome.terrainSprites.Length > 0)
        {
            List<TerrainType> allowed = new List<TerrainType>();
            foreach (var set in biome.terrainSprites)
            {
                switch (set.terrainType)
                {
                    case TerrainType.Ocean:
                    case TerrainType.Wall:
                    case TerrainType.Gate:
                    case TerrainType.Ladder:
                    case TerrainType.Town:
                    case TerrainType.Cliff:
                        break;
                    default:
                        allowed.Add(set.terrainType);
                        break;
                }
            }

            if (allowed.Count > 0)
            {
                float mainChance = 0.5f + (float)rng.NextDouble() * 0.1f; // 50-60%
                TerrainType main = allowed[0];
                if (rng.NextDouble() < mainChance)
                    return main;

                int idx = rng.Next(allowed.Count);
                return allowed[idx];
            }
        }

        return TerrainType.Grass;
    }

    List<Vector2Int> GenerateCluster(TerrainType type, int size, int margin = 1)
    {
        List<Vector2Int> created = new List<Vector2Int>();
        int startX = rng.Next(margin, width - margin);
        int startY = rng.Next(margin, height - margin);
        Vector2Int pos = new Vector2Int(startX, startY);
        created.Add(pos);
        SetCellTerrain(pos.x, pos.y, type);

        for (int i = 1; i < size; i++)
        {
            Vector2Int cur = created[rng.Next(created.Count)];
            List<Vector2Int> neighbors = new List<Vector2Int>();
            if (cur.x > margin) neighbors.Add(new Vector2Int(cur.x - 1, cur.y));
            if (cur.x < width - margin - 1) neighbors.Add(new Vector2Int(cur.x + 1, cur.y));
            if (cur.y > margin) neighbors.Add(new Vector2Int(cur.x, cur.y - 1));
            if (cur.y < height - margin - 1) neighbors.Add(new Vector2Int(cur.x, cur.y + 1));
            Vector2Int next = neighbors[rng.Next(neighbors.Count)];
            if (!created.Contains(next))
            {
                created.Add(next);
                SetCellTerrain(next.x, next.y, type);
            }
        }
        return created;
    }

    void GenerateRiver(Vector2Int start, Vector2Int end)
    {
        Vector2Int cur = start;
        SetCellTerrain(cur.x, cur.y, TerrainType.River);
        while (cur != end)
        {
            if (cur.x < end.x) cur.x++; else if (cur.x > end.x) cur.x--;
            else if (cur.y < end.y) cur.y++; else if (cur.y > end.y) cur.y--;

            if (cells[cur.x, cur.y].terrainType == TerrainType.Road)
                SetCellTerrain(cur.x, cur.y, TerrainType.Bridge);
            else
                SetCellTerrain(cur.x, cur.y, TerrainType.River);
        }
    }

    void GenerateRoad(Vector2Int start, Vector2Int end)
    {
        Vector2Int cur = start;
        SetCellTerrain(cur.x, cur.y, TerrainType.Road);
        while (cur != end)
        {
            if (cur.x < end.x) cur.x++; else if (cur.x > end.x) cur.x--;
            else if (cur.y < end.y) cur.y++; else if (cur.y > end.y) cur.y--;

            if (cells[cur.x, cur.y].terrainType == TerrainType.River)
                SetCellTerrain(cur.x, cur.y, TerrainType.Bridge);
            else
                SetCellTerrain(cur.x, cur.y, TerrainType.Road);
        }
    }

    void GenerateGrid()
    {
        rng = new System.Random(seed);

        float xOffset = -((width - 1) * cellSize) / 2f;
        float yOffset = -((height - 1) * cellSize) / 2f;

        cells = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = new GameObject($"Cell_{x}_{y}");
                cell.transform.position = new Vector3(x * cellSize + xOffset, y * cellSize + yOffset, 0);
                cell.transform.parent = transform;

                var spriteRenderer = cell.AddComponent<SpriteRenderer>();
                spriteRenderer.color = Color.white;
                if (cellMaterial != null)
                    spriteRenderer.material = cellMaterial;

                cell.transform.localScale = Vector3.one;

                cell.AddComponent<BoxCollider2D>().size = new Vector2(cellSize, cellSize);

                Cell cellScript = cell.AddComponent<Cell>();
                cells[x, y] = cellScript;

                TerrainType t;
                if (usePerlinNoise)
                    t = ChooseTerrainType(x, y);
                else
                    t = ChooseTerrainType();
                SetCellTerrain(x, y, t);
            }
        }

        GenerateFeatures();
    }

    void GenerateFeatures()
    {
        int level = DungeonProgressionManager.Instance != null ? DungeonProgressionManager.Instance.CurrentLevel : 1;

        // Lakes with surrounding cliffs
        int lakeCount = rng.Next(1, 3) + level / 7;
        for (int i = 0; i < lakeCount; i++)
        {
            var lake = GenerateCluster(TerrainType.Ocean, rng.Next(4, 8), 2);
            foreach (var p in lake)
            {
                Vector2Int[] deltas = { new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(-1,0) };
                foreach (var d in deltas)
                {
                    Vector2Int n = p + d;
                    if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height)
                    {
                        if (cells[n.x, n.y].terrainType == TerrainType.Grass)
                            SetCellTerrain(n.x, n.y, TerrainType.Cliff);
                    }
                }
            }
        }

        // Forest clusters
        int forestClusters = rng.Next(3, 6) + level / 6;
        for (int i = 0; i < forestClusters; i++)
            GenerateCluster(TerrainType.Forest, rng.Next(6, 12), 1);

        // Mountain clusters with hills around
        int mountainClusters = rng.Next(1, 3) + level / 8;
        for (int i = 0; i < mountainClusters; i++)
        {
            var mts = GenerateCluster(TerrainType.Mountain, rng.Next(3, 6), 2);
            foreach (var p in mts)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        Vector2Int n = new Vector2Int(p.x + dx, p.y + dy);
                        if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height)
                        {
                            if (cells[n.x, n.y].terrainType == TerrainType.Grass)
                                SetCellTerrain(n.x, n.y, TerrainType.Hill);
                        }
                    }
                }
            }
        }

        // Snow fields
        int snowClusters = rng.Next(1, 3) + level / 10;
        for (int i = 0; i < snowClusters; i++)
            GenerateCluster(TerrainType.Snow, rng.Next(2, 5), 1);

        // Rivers
        int riverCount = rng.Next(1, 3);
        for (int i = 0; i < riverCount; i++)
        {
            Vector2Int start = new Vector2Int(rng.Next(width), rng.Next(2) == 0 ? 0 : height - 1);
            Vector2Int end = new Vector2Int(rng.Next(width), start.y == 0 ? height - 1 : 0);
            if (rng.Next(2) == 0)
            {
                start = new Vector2Int(0, rng.Next(height));
                end = new Vector2Int(width - 1, rng.Next(height));
            }
            GenerateRiver(start, end);
        }

        // Fortress with walls, gates and town tiles
        if (width > 6 && height > 6)
        {
            int cx = rng.Next(3, width - 3);
            int cy = rng.Next(3, height - 3);

            for (int x = cx - 1; x <= cx + 1; x++)
                for (int y = cy - 1; y <= cy + 1; y++)
                    SetCellTerrain(x, y, TerrainType.Town);

            for (int x = cx - 2; x <= cx + 2; x++)
            {
                for (int y = cy - 2; y <= cy + 2; y++)
                {
                    bool border = x == cx - 2 || x == cx + 2 || y == cy - 2 || y == cy + 2;
                    if (border)
                        SetCellTerrain(x, y, TerrainType.Wall);
                }
            }

            // gates
            int gates = rng.Next(1, 3);
            for (int g = 0; g < gates; g++)
            {
                int side = rng.Next(4);
                Vector2Int pos;
                if (side == 0) pos = new Vector2Int(cx, cy - 2);
                else if (side == 1) pos = new Vector2Int(cx, cy + 2);
                else if (side == 2) pos = new Vector2Int(cx - 2, cy);
                else pos = new Vector2Int(cx + 2, cy);
                SetCellTerrain(pos.x, pos.y, TerrainType.Gate);
            }
        }

        // Additional villages
        if (level > 2 && width > 8 && height > 8)
        {
            int villages = rng.Next(1, 1 + level / 5);
            for (int v = 0; v < villages; v++)
                GenerateVillage();
        }

        // Occasional ruins
        if (level > 4 && width > 10 && height > 10 && rng.NextDouble() < 0.4)
        {
            GenerateRuins();
        }
    }

    void GenerateVillage()
    {
        int cx = rng.Next(2, width - 2);
        int cy = rng.Next(2, height - 2);
        for (int x = cx - 1; x <= cx + 1; x++)
            for (int y = cy - 1; y <= cy + 1; y++)
                SetCellTerrain(x, y, TerrainType.Town);

        GenerateRoad(new Vector2Int(cx - 2, cy), new Vector2Int(cx + 2, cy));
        GenerateRoad(new Vector2Int(cx, cy - 2), new Vector2Int(cx, cy + 2));
    }

    void GenerateRuins()
    {
        int cx = rng.Next(2, width - 2);
        int cy = rng.Next(2, height - 2);

        for (int x = cx - 1; x <= cx + 1; x++)
            for (int y = cy - 1; y <= cy + 1; y++)
                SetCellTerrain(x, y, TerrainType.Grass);

        for (int x = cx - 2; x <= cx + 2; x++)
        {
            for (int y = cy - 2; y <= cy + 2; y++)
            {
                bool border = x == cx - 2 || x == cx + 2 || y == cy - 2 || y == cy + 2;
                if (border && rng.NextDouble() > 0.3)
                    SetCellTerrain(x, y, TerrainType.Wall);
            }
        }
    }


    public Vector3 GetCellCenterPosition(int x, int y)
    {
        float xOffset = -((width - 1) * cellSize) / 2f;
        float yOffset = -((height - 1) * cellSize) / 2f;
        return new Vector3(x * cellSize + xOffset, y * cellSize + yOffset, 0);
    }
    public List<Cell> GetCellsInRange(Vector2Int center, int range)
    {
        List<Cell> result = new List<Cell>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int dist = Mathf.Abs(center.x - x) + Mathf.Abs(center.y - y);
                if (dist <= range)
                    result.Add(cells[x, y]);
            }
        }
        return result;
    }

    // Переводим worldPosition -> координаты клетки
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x + width / 2f - 0.5f);
        int y = Mathf.RoundToInt(worldPos.y + height / 2f - 0.5f);
        return new Vector2Int(x, y);
    }

    public Cell GetCellFromWorld(Vector3 worldPos)
    {
        Vector2Int grid = WorldToGrid(worldPos);
        if (grid.x < 0 || grid.x >= width || grid.y < 0 || grid.y >= height)
            return null;
        return cells[grid.x, grid.y];
    }

    public void PlaceEntryExit(Vector2Int entry, Vector2Int exit, bool firstLevel)
    {
        entryPoint = entry;
        exitPoint = exit;
        exitUnlocked = false;

        SetCellTerrain(exitPoint.x, exitPoint.y, TerrainType.Road);
        SetCellTerrain(entryPoint.x, entryPoint.y, firstLevel ? TerrainType.Gate : TerrainType.Road);
        cells[entryPoint.x, entryPoint.y].SetBaseColor(entryHighlight);
        cells[exitPoint.x, exitPoint.y].SetBaseColor(exitHighlight);

        if (firstLevel)
            BuildEntryWalls();

        int dist = Mathf.Abs(entryPoint.x - exitPoint.x) + Mathf.Abs(entryPoint.y - exitPoint.y);
        if (dist < 6)
            AddBlockingRidge();

        GenerateRoadPath();
    }

    void AddBlockingRidge()
    {
        int ridgeX = width / 2;
        int gapY = Random.Range(1, height - 2);
        for (int y = 1; y < height - 1; y++)
        {
            if (y == gapY) continue;
            SetCellTerrain(ridgeX, y, TerrainType.Mountain);
        }
    }

    public void UnlockExit()
    {
        exitUnlocked = true;
    }

    void BuildEntryWalls()
    {
        if (entryPoint.x == 0 || entryPoint.x == width - 1)
        {
            int x = entryPoint.x;
            for (int y = 0; y < height; y++)
            {
                if (y == entryPoint.y) continue;
                SetCellTerrain(x, y, TerrainType.Wall);
            }
        }
        else if (entryPoint.y == 0 || entryPoint.y == height - 1)
        {
            int y = entryPoint.y;
            for (int x = 0; x < width; x++)
            {
                if (x == entryPoint.x) continue;
                SetCellTerrain(x, y, TerrainType.Wall);
            }
        }
    }

    bool IsRoadTraversable(Vector2Int pos)
    {
        TerrainType t = cells[pos.x, pos.y].terrainType;
        if (t == TerrainType.Wall || t == TerrainType.Mountain || t == TerrainType.Cliff || t == TerrainType.Ocean)
            return false;
        return true;
    }

    List<Vector2Int> GetRoadNeighbors(Vector2Int pos)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        Vector2Int[] deltas = { new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(-1,0) };
        foreach (var d in deltas)
        {
            Vector2Int n = pos + d;
            if (n.x < 0 || n.x >= width || n.y < 0 || n.y >= height)
                continue;
            if (IsRoadTraversable(n))
                list.Add(n);
        }
        return list;
    }

    int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector2Int> Reconstruct(Vector2Int current, Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    List<Vector2Int> FindRoadPath(Vector2Int start, Vector2Int goal)
    {
        var open = new SimplePriorityQueue<Vector2Int, int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int>();
        var fScore = new Dictionary<Vector2Int, int>();

        open.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (open.Count > 0)
        {
            Vector2Int current = open.Dequeue();
            if (current == goal)
                return Reconstruct(current, cameFrom);

            foreach (var n in GetRoadNeighbors(current))
            {
                int tentative = gScore[current] + 1;
                if (!gScore.ContainsKey(n) || tentative < gScore[n])
                {
                    cameFrom[n] = current;
                    gScore[n] = tentative;
                    int pri = tentative + Heuristic(n, goal);
                    fScore[n] = pri;
                    if (!open.Contains(n))
                        open.Enqueue(n, pri);
                    else
                        open.UpdatePriority(n, pri);
                }
            }
        }
        return null;
    }

    void GenerateRoadPath()
    {
        var path = FindRoadPath(entryPoint, exitPoint);
        if (path == null) return;
        foreach (var p in path)
        {
            if (p == entryPoint || p == exitPoint) continue;
            if (cells[p.x, p.y].terrainType == TerrainType.River)
                SetCellTerrain(p.x, p.y, TerrainType.Bridge);
            else
                SetCellTerrain(p.x, p.y, TerrainType.Road);
        }
    }

    public bool IsExitCell(Cell cell)
    {
        Vector2Int pos = WorldToGrid(cell.transform.position);
        return pos == exitPoint;
    }

    public bool IsExitUnlocked => exitUnlocked;
}
