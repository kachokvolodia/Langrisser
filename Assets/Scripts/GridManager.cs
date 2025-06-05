using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        Instance = this;
        if (randomSeed)
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        GenerateGrid();
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
            case TerrainType.Ocean:
            case TerrainType.Road:
            case TerrainType.Bridge:
            case TerrainType.Town:
                return 1;
            case TerrainType.Wall:
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
                SetCellTerrain(x, y, TerrainType.Grass);
            }
        }

        // Ocean lake
        List<Vector2Int> ocean = GenerateCluster(TerrainType.Ocean, 5, 2);

        // River to lake
        Vector2Int riverStart = new Vector2Int(rng.Next(1, width - 1), height - 2);
        GenerateRiver(riverStart, ocean[0]);

        // Desert and forest clusters
        GenerateCluster(TerrainType.Desert, 6, 2);
        GenerateCluster(TerrainType.Forest, 8, 2);

        // Internal mountain mass
        List<Vector2Int> mountains = GenerateCluster(TerrainType.Mountain, 4, 2);

        // Hills around mountains
        foreach (var m in mountains)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = m.x + dx; int ny = m.y + dy;
                    if (nx < 1 || ny < 1 || nx >= width - 1 || ny >= height - 1) continue;
                    if (cells[nx, ny].terrainType == TerrainType.Grass)
                        SetCellTerrain(nx, ny, TerrainType.Hill);
                }
            }
        }

        // Outer mountains as borders
        for (int x = 0; x < width; x++)
        {
            SetCellTerrain(x, 0, TerrainType.Mountain);
            SetCellTerrain(x, height - 1, TerrainType.Mountain);
        }
        for (int y = 0; y < height; y++)
        {
            SetCellTerrain(0, y, TerrainType.Mountain);
            SetCellTerrain(width - 1, y, TerrainType.Mountain);
        }

        // Castle and road to border
        GenerateCastle(width / 2 - 3, height / 2 - 3, 6, 6);
        int gateX = width / 2;
        Vector2Int gate = new Vector2Int(gateX, height / 2 - 3);
        GenerateRoad(gate, new Vector2Int(gateX, 1));
    }

    void GenerateCastle(int startX, int startY, int castleWidth, int castleHeight)
    {
        for (int x = 0; x < castleWidth; x++)
        {
            for (int y = 0; y < castleHeight; y++)
            {
                int gx = startX + x;
                int gy = startY + y;
                if (gx < 0 || gx >= width || gy < 0 || gy >= height)
                    continue;

                Cell cell = cells[gx, gy];
                if (x == 0 || x == castleWidth - 1 || y == 0 || y == castleHeight - 1)
                {
                    cell.terrainType = TerrainType.Wall;
                    cell.moveCost = 99;
                }
                else
                {
                    cell.terrainType = TerrainType.Road;
                    cell.moveCost = 1;
                }
                cell.GetComponent<SpriteRenderer>().sprite = GetSpriteForType(cell.terrainType);
            }
        }

        int gateX = startX + castleWidth / 2;
        int gateY = startY;
        if (gateX >= 0 && gateX < width && gateY >= 0 && gateY < height)
        {
            cells[gateX, gateY].terrainType = TerrainType.Road;
            cells[gateX, gateY].moveCost = 1;
            cells[gateX, gateY].GetComponent<SpriteRenderer>().sprite = GetSpriteForType(TerrainType.Road);
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
}
