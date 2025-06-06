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

    // Seed for deterministic generation
    public int seed = 0;
    public bool randomSeed = true;

    public Cell[,] cells;

    private System.Random rng;

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

    TerrainType ChooseTerrainType()
    {
        if (biome != null && biome.terrainSprites != null && biome.terrainSprites.Length > 0)
        {
            float mainChance = 0.5f + (float)rng.NextDouble() * 0.1f; // 50-60%
            if (rng.NextDouble() < mainChance)
                return biome.terrainSprites[0].terrainType;

            if (biome.terrainSprites.Length > 1)
            {
                int idx = rng.Next(1, biome.terrainSprites.Length);
                return biome.terrainSprites[idx].terrainType;
            }
            return biome.terrainSprites[0].terrainType;
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

                TerrainType t = ChooseTerrainType();
                SetCellTerrain(x, y, t);
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

    public bool ExitUnlocked => exitUnlocked;
}
