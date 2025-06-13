using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public class PathfindingTests
{
    [Test]
    public void Heuristic_ReturnsManhattanDistance()
    {
        var go = new GameObject();
        var mgr = go.AddComponent<PathfindingManager>();
        var a = new Cell { gridPos = new Vector2Int(0, 0) };
        var b = new Cell { gridPos = new Vector2Int(2, 3) };
        MethodInfo m = typeof(PathfindingManager).GetMethod("HeuristicCostEstimate", BindingFlags.NonPublic | BindingFlags.Instance);
        int dist = (int)m.Invoke(mgr, new object[] { a, b });
        Assert.AreEqual(5, dist);
        Object.DestroyImmediate(go);
    }
}
