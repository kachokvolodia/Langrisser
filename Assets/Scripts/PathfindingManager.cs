using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // A* для клеточного поля
    public List<Cell> FindPath(Cell start, Cell goal, Unit unit)
    {
        var openSet = new SimplePriorityQueue<Cell>();
        var cameFrom = new Dictionary<Cell, Cell>();

        var gScore = new Dictionary<Cell, int>();
        var fScore = new Dictionary<Cell, int>();

        // Кладём стартовую клетку
        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = HeuristicCostEstimate(start, goal);

        while (openSet.Count > 0)
        {
            Cell current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (Cell neighbor in GetNeighbors(current, unit))
            {
                if (!neighbor.IsPassable(unit) && neighbor != goal)
                    continue;
                int tentativeGScore = gScore[current] + neighbor.GetMoveCost(unit);
                if (neighbor.occupyingUnit != null && neighbor != goal)
                    continue; // Занятая клетка — нельзя!

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + HeuristicCostEstimate(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    else
                        openSet.UpdatePriority(neighbor, fScore[neighbor]);
                }
            }
        }
        return null; // Нет пути
    }

    int HeuristicCostEstimate(Cell a, Cell b)
    {
        // Манхэттенское расстояние (или диагональное, если хочешь)
        return Mathf.Abs(GridManager.Instance.WorldToGrid(a.transform.position).x - GridManager.Instance.WorldToGrid(b.transform.position).x)
             + Mathf.Abs(GridManager.Instance.WorldToGrid(a.transform.position).y - GridManager.Instance.WorldToGrid(b.transform.position).y);
    }

    List<Cell> ReconstructPath(Dictionary<Cell, Cell> cameFrom, Cell current)
    {
        List<Cell> totalPath = new List<Cell> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    List<Cell> GetNeighbors(Cell cell, Unit unit)
    {
        List<Cell> neighbors = new List<Cell>();
        Vector2Int[] deltas = {
            new Vector2Int(0,1), new Vector2Int(1,0),
            new Vector2Int(0,-1), new Vector2Int(-1,0),
            new Vector2Int(1,1), new Vector2Int(-1,1),
            new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };
        Vector2Int pos = GridManager.Instance.WorldToGrid(cell.transform.position);
        foreach (var delta in deltas)
        {
            Vector2Int np = pos + delta;
            if (np.x < 0 || np.x >= GridManager.Instance.width || np.y < 0 || np.y >= GridManager.Instance.height)
                continue;

            var nCell = GridManager.Instance.cells[np.x, np.y];
            // Тут можно фильтровать по типу юнита: если не плавает, не пропускать воду и т.д.
            if (unit != null /* && !unit.CanPass(nCell) */) { }
            neighbors.Add(nCell);
        }
        return neighbors;
    }

    public List<Cell> GetReachableCells(Cell start, int movePoints, Unit unit)
    {
        var result = new List<Cell>();
        var queue = new Queue<(Cell cell, int cost)>();
        var visited = new Dictionary<Cell, int>();
        queue.Enqueue((start, 0));
        visited[start] = 0;

        while (queue.Count > 0)
        {
            var (cell, cost) = queue.Dequeue();
            foreach (var n in GetNeighbors(cell, unit))
            {
                if (!n.IsPassable(unit)) continue;
                int newCost = cost + n.GetMoveCost(unit);
                if (newCost > movePoints) continue;
                if (!visited.ContainsKey(n) || newCost < visited[n])
                {
                    visited[n] = newCost;
                    queue.Enqueue((n, newCost));
                    if (n != start && n.occupyingUnit == null)
                        result.Add(n);
                }
            }
        }
        return result;
    }
}