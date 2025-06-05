using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        GenerateGrid();
    }

    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public Sprite[] tileSprites;
    public Material cellMaterial;
    public Cell[,] cells;
    void Start()
    {
    }
    void GenerateGrid()
    {
        float xOffset = -((width - 1) * cellSize) / 2f;
        float yOffset = -((height - 1) * cellSize) / 2f;

        cells = new Cell[width, height]; // ← вот здесь, ПЕРЕД циклом!

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = new GameObject($"Cell_{x}_{y}");
                cell.transform.position = new Vector3(x * cellSize + xOffset, y * cellSize + yOffset, 0);
                cell.transform.parent = transform;

                var spriteRenderer = cell.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = tileSprites[Random.Range(0, tileSprites.Length)];
                spriteRenderer.color = Color.white;
                if (cellMaterial != null)
                    spriteRenderer.material = cellMaterial;

                cell.transform.localScale = Vector3.one;

                cell.AddComponent<BoxCollider2D>().size = new Vector2(cellSize, cellSize);

                Cell cellScript = cell.AddComponent<Cell>();

                // Пример рандомного типа местности
                int r = Random.Range(0, 5);
                switch (r)
                {
                    case 0: cellScript.terrainType = TerrainType.Grass; cellScript.moveCost = 1; break;
                    case 1: cellScript.terrainType = TerrainType.Forest; cellScript.moveCost = 2; break;
                    case 2: cellScript.terrainType = TerrainType.Hill; cellScript.moveCost = 2; break;
                    case 3: cellScript.terrainType = TerrainType.Mountain; cellScript.moveCost = 3; break;
                    case 4: cellScript.terrainType = TerrainType.Ocean; cellScript.moveCost = 1; break;
                }

                cells[x, y] = cellScript; // заполняем массив
            }
        }

        // После базовой генерации можно добавить структуры
        GenerateCastle(width / 2 - 3, height / 2 - 3, 6, 6);
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
            }
        }

        int gateX = startX + castleWidth / 2;
        int gateY = startY;
        if (gateX >= 0 && gateX < width && gateY >= 0 && gateY < height)
        {
            cells[gateX, gateY].terrainType = TerrainType.Road;
            cells[gateX, gateY].moveCost = 1;
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
