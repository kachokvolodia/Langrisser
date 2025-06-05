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

    public void PlaceEntryExit(Vector2Int entry, Vector2Int exit)
    {
        entryPoint = entry;
        exitPoint = exit;
        exitUnlocked = false;
        SetCellTerrain(entryPoint.x, entryPoint.y, TerrainType.Town);
        SetCellTerrain(exitPoint.x, exitPoint.y, TerrainType.Road);

        int dist = Mathf.Abs(entryPoint.x - exitPoint.x) + Mathf.Abs(entryPoint.y - exitPoint.y);
        if (dist < 6)
            AddBlockingRidge();
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

    public bool IsExitCell(Cell cell)
    {
        Vector2Int pos = WorldToGrid(cell.transform.position);
        return pos == exitPoint;
    }

    public bool ExitUnlocked => exitUnlocked;
}
