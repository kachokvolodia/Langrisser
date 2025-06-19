using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Priority_Queue;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;
    public Biome biome;

    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap terrainTilemap;
    public Tilemap objectTilemap;
    public TileBase groundTile;

    [Header("Noise Settings")]
    [SerializeField] private bool usePerlinNoise = true;
    [SerializeField] private float terrainNoiseScale = 0.1f;
    [SerializeField] private float objectNoiseScale = 0.2f;

    // Seed for deterministic generation
    [SerializeField] private int seed = 0;
    [SerializeField] private bool randomSeed = true;

    public Cell[,] cells;

    private System.Random rng;

    [SerializeField] private Color entryHighlight = new Color(0.8f, 1f, 0.6f);
    [SerializeField] private Color exitHighlight = new Color(1f, 0.6f, 0.6f);

    public Vector2Int entryPoint;
    public Vector2Int exitPoint;
    private bool exitUnlocked = false;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public bool UsePerlinNoise => usePerlinNoise;
    public float TerrainNoiseScale => terrainNoiseScale;
    public float ObjectNoiseScale => objectNoiseScale;
    public int Seed => seed;
    public bool RandomSeed => randomSeed;
    public Color EntryHighlight => entryHighlight;
    public Color ExitHighlight => exitHighlight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (randomSeed)
            seed = UnityEngine.Random.Range(0, 100000);
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
        // При переходе на систему Tilemap пока не генерируем дополнительные
        // особенности ландшафта вроде лесов и гор
        GenerateFeatures();
    }

    void ClearGrid()
    {
        if (cells == null) return;
        groundTilemap.ClearAllTiles();
        terrainTilemap.ClearAllTiles();
        objectTilemap.ClearAllTiles();
        cells = null;
    }
    void Start()
    {
    }
    TileBase GetTileForType(TerrainType type)
    {
        if (biome != null)
        {
            TileBase t = biome.GetTile(type);
            if (t != null) return t;
        }
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
            case TerrainType.BridgeHorizontal:
            case TerrainType.BridgeVertical:
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

        TileBase tile = GetTileForType(tType);
        if (tile != null)
            terrainTilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }

    void SetObjectTerrain(int x, int y, TerrainType tType)
    {
        Cell cell = cells[x, y];
        cell.terrainType = tType;
        cell.moveCost = GetMoveCostForType(tType);

        TileBase tile = GetTileForType(tType);
        if (tile != null)
            objectTilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }

    TerrainType ChooseTerrainType(int x, int y)
    {
        float noise = Mathf.PerlinNoise((x + seed) * terrainNoiseScale,
                                        (y + seed) * terrainNoiseScale);
        return noise < 0.3f ? TerrainType.Ocean : TerrainType.Grass;
    }

    TerrainType ChooseTerrainType()
    {
        if (biome != null && biome.terrainTiles != null && biome.terrainTiles.Length > 0)
        {
            List<TerrainType> allowed = new List<TerrainType>();
            foreach (var set in biome.terrainTiles)
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
        Vector2Int prev = cur;
        SetObjectTerrain(cur.x, cur.y, TerrainType.River);
        while (cur != end)
        {
            prev = cur;
            if (cur.x < end.x) cur.x++; else if (cur.x > end.x) cur.x--;
            else if (cur.y < end.y) cur.y++; else if (cur.y > end.y) cur.y--;

            bool verticalStep = cur.y != prev.y;
            if (cells[cur.x, cur.y].terrainType == TerrainType.Road)
                SetObjectTerrain(cur.x, cur.y, verticalStep ? TerrainType.BridgeVertical : TerrainType.BridgeHorizontal);
            else
                SetObjectTerrain(cur.x, cur.y, TerrainType.River);
        }
    }

    void GenerateRoad(Vector2Int start, Vector2Int end)
    {
        Vector2Int cur = start;
        SetObjectTerrain(cur.x, cur.y, TerrainType.Road);
        while (cur != end)
        {
            if (cur.x < end.x) cur.x++; else if (cur.x > end.x) cur.x--;
            else if (cur.y < end.y) cur.y++; else if (cur.y > end.y) cur.y--;

            if (cells[cur.x, cur.y].terrainType == TerrainType.River)
                SetObjectTerrain(cur.x, cur.y, GetBridgeTypeForRiver(new Vector2Int(cur.x, cur.y)));
            else
                SetObjectTerrain(cur.x, cur.y, TerrainType.Road);
        }
    }

    void GenerateGrid()
    {
        rng = new System.Random(seed);

        groundTilemap.ClearAllTiles();
        terrainTilemap.ClearAllTiles();
        objectTilemap.ClearAllTiles();

        cells = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.gridPos = new Vector2Int(x, y);
                cell.worldPos = GetCellCenterPosition(x, y);
                cells[x, y] = cell;

                if (groundTile != null)
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);

                TerrainType type = TerrainType.Grass;
                if (usePerlinNoise)
                    type = ChooseTerrainType(x, y);
                SetCellTerrain(x, y, type);
            }
        }

        terrainTilemap.RefreshAllTiles();
        objectTilemap.RefreshAllTiles();
    }

    void GenerateFeatures()
    {
        // В новой системе Tilemap ландшафт пока состоит только из базовых тайлов,
        // поэтому дополнительных элементов (леса, горы и т.п.) не создаём.
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
        Vector3Int cell = new Vector3Int(x, y, 0);
        return groundTilemap.GetCellCenterWorld(cell);
    }

    public void SetTileColor(Vector2Int pos, Color color)
    {
        Vector3Int p = new Vector3Int(pos.x, pos.y, 0);
        terrainTilemap.SetTileFlags(p, TileFlags.None);
        terrainTilemap.SetColor(p, color);
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
        Vector3Int cell = groundTilemap.WorldToCell(worldPos);
        return new Vector2Int(cell.x, cell.y);
    }

    public Cell GetCellFromWorld(Vector3 worldPos)
    {
        Vector2Int grid = WorldToGrid(worldPos);
        if (grid.x < 0 || grid.x >= width || grid.y < 0 || grid.y >= height)
            return null;
        return cells[grid.x, grid.y];
    }

    void EnsureClearAround(Vector2Int pos)
    {
        Vector2Int[] deltas =
        {
            new Vector2Int(0, 1), new Vector2Int(1, 0),
            new Vector2Int(0, -1), new Vector2Int(-1, 0)
        };
        foreach (var d in deltas)
        {
            Vector2Int n = pos + d;
            if (n.x < 0 || n.x >= width || n.y < 0 || n.y >= height)
                continue;
            if (!IsRoadTraversable(n))
                SetCellTerrain(n.x, n.y, TerrainType.Grass);
        }
    }

    public void PlaceEntryExit(Vector2Int entry, Vector2Int exit, bool firstLevel)
    {
        entryPoint = entry;
        exitPoint = exit;
        exitUnlocked = false;
        // Очищаем клетки от горного хребта и размечаем вход и выход
        SetCellTerrain(entryPoint.x, entryPoint.y, TerrainType.Road);
        SetCellTerrain(exitPoint.x, exitPoint.y, TerrainType.Road);
        EnsureClearAround(entryPoint);
        EnsureClearAround(exitPoint);
        cells[entryPoint.x, entryPoint.y].SetBaseColor(entryHighlight);
        cells[exitPoint.x, exitPoint.y].SetBaseColor(exitHighlight);
    }

    public void AddBlockingRidge()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool border = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                if (border)
                {
                    if ((entryPoint == new Vector2Int(x, y)) || (exitPoint == new Vector2Int(x, y)))
                        continue;
                    SetCellTerrain(x, y, TerrainType.Mountain);
                }
            }
        }
    }

    public void UnlockExit()
    {
        exitUnlocked = true;
    }

    void BuildEntryWalls()
    {
        // Стены входа временно не строим
    }

    bool IsRiverTraversable(Vector2Int pos)
    {
        TerrainType t = cells[pos.x, pos.y].terrainType;
        if (t == TerrainType.Wall || t == TerrainType.Mountain || t == TerrainType.Cliff)
            return false;
        return true;
    }

    List<Vector2Int> GetRiverNeighbors(Vector2Int pos)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        Vector2Int[] deltas = { new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(-1,0) };
        foreach (var d in deltas)
        {
            Vector2Int n = pos + d;
            if (n.x < 0 || n.x >= width || n.y < 0 || n.y >= height)
                continue;
            if (IsRiverTraversable(n))
                list.Add(n);
        }
        return list;
    }

    TerrainType GetBridgeTypeForRiver(Vector2Int pos)
    {
        bool up = pos.y + 1 < height && cells[pos.x, pos.y + 1].terrainType == TerrainType.River;
        bool down = pos.y - 1 >= 0 && cells[pos.x, pos.y - 1].terrainType == TerrainType.River;
        bool left = pos.x - 1 >= 0 && cells[pos.x - 1, pos.y].terrainType == TerrainType.River;
        bool right = pos.x + 1 < width && cells[pos.x + 1, pos.y].terrainType == TerrainType.River;

        int verticalCount = (up ? 1 : 0) + (down ? 1 : 0);
        int horizontalCount = (left ? 1 : 0) + (right ? 1 : 0);

        return verticalCount >= horizontalCount ? TerrainType.BridgeVertical : TerrainType.BridgeHorizontal;
    }

    List<Vector2Int> FindRiverPath(Vector2Int start, Vector2Int goal)
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

            foreach (var n in GetRiverNeighbors(current))
            {
                int tentative = gScore[current] + 1 + rng.Next(0, 3);
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

    Vector2Int GetRandomEdgeCell()
    {
        int side = rng.Next(4);
        switch (side)
        {
            case 0: return new Vector2Int(1, rng.Next(1, height - 1));
            case 1: return new Vector2Int(width - 2, rng.Next(1, height - 1));
            case 2: return new Vector2Int(rng.Next(1, width - 1), 1);
            default: return new Vector2Int(rng.Next(1, width - 1), height - 2);
        }
    }

    Vector2Int GetRandomOceanCell()
    {
        List<Vector2Int> oceans = new List<Vector2Int>();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (cells[x, y].terrainType == TerrainType.Ocean)
                    oceans.Add(new Vector2Int(x, y));
        if (oceans.Count > 0)
            return oceans[rng.Next(oceans.Count)];
        return GetRandomEdgeCell();
    }

    void GenerateRiverPath(Vector2Int start, Vector2Int end)
    {
        var path = FindRiverPath(start, end);
        if (path == null) return;
        foreach (var p in path)
        {
            if (cells[p.x, p.y].terrainType == TerrainType.Ocean)
                break;
            SetObjectTerrain(p.x, p.y, TerrainType.River);
        }
    }

    public void GenerateRiverNetwork()
    {
        int riverCount = rng.Next(1, 3);
        for (int i = 0; i < riverCount; i++)
        {
            Vector2Int start = GetRandomEdgeCell();
            Vector2Int end = GetRandomOceanCell();
            GenerateRiverPath(start, end);

            if (rng.NextDouble() < 0.3)
            {
                var mainPath = FindRiverPath(start, end);
                if (mainPath != null && mainPath.Count > 6)
                {
                    int idx = rng.Next(3, mainPath.Count - 3);
                    Vector2Int branchStart = mainPath[idx];
                    Vector2Int branchEnd = rng.NextDouble() < 0.5 ? GetRandomOceanCell() : GetRandomEdgeCell();
                    GenerateRiverPath(branchStart, branchEnd);
                }
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
            bool border = n.x == 0 || n.x == width - 1 || n.y == 0 || n.y == height - 1;
            if (border && n != entryPoint && n != exitPoint)
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
                int cost = 1;
                if (cells[n.x, n.y].terrainType == TerrainType.River)
                    cost += 5;
                int tentative = gScore[current] + cost;
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

    public void GenerateRoadPath()
    {
        var path = FindRoadPath(entryPoint, exitPoint);
        if (path == null || path.Count == 0)
        {
            GenerateRoad(entryPoint, exitPoint);
            return;
        }
        foreach (var p in path)
        {
            if (p == entryPoint || p == exitPoint) continue;
            if (cells[p.x, p.y].terrainType == TerrainType.River)
                SetObjectTerrain(p.x, p.y, GetBridgeTypeForRiver(p));
            else
                SetObjectTerrain(p.x, p.y, TerrainType.Road);
        }
    }

    public bool IsExitCell(Cell cell)
    {
        return cell.gridPos == exitPoint;
    }

    public bool IsExitUnlocked => exitUnlocked;
}
