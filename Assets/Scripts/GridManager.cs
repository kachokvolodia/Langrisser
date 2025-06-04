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
                cells[x, y] = cellScript; // ← добавь вот эту строку для заполнения массива!
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
}
