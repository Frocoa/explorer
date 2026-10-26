using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates the corridor layout of a maze floor, following the same rules
/// as MyHordes' MazeMaker::generateMaze(). Pure data generation: no rendering.
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    [Header("Grid size (MyHordes default values)")]
    [SerializeField] private int sizeX = 13;
    [SerializeField] private int sizeY = 13;
    [SerializeField] private int offsetX = 7;

    [Header("Generation behaviour")]
    [SerializeField] private float joinPathProbability = 0.2f;
    [SerializeField] private int minStepsBeforeRedirect = 3;

    [Header("Debug")]
    [SerializeField] private int seed = 0;

    public class Cell
    {
        public int x, y;
        public bool open;          // is corridor
        public bool N, S, E, W;    // open sides, computed at the end
        public int dist = int.MaxValue; // shortest path distance (in steps) from the entrance
    }

    private Dictionary<(int, int), Cell> grid;
    private System.Random rng;
    private bool generated;

    public Dictionary<(int, int), Cell> Grid => grid;

    private Cell At(int x, int y)
    {
        grid.TryGetValue((x, y), out var c);
        return c;
    }

    // -----------------------------------------------------------------
    // 1. Create the empty grid (all walls) + the entrance
    // -----------------------------------------------------------------
    private void CreateGrid()
    {
        grid = new Dictionary<(int, int), Cell>();

        // Entrance, outside the main grid, same as the original
        grid[(0, 0)] = new Cell { x = 0, y = 0, open = true };

        for (int x = -offsetX; x < -offsetX + sizeX; x++)
            for (int y = 1; y < 1 + sizeY; y++)
                grid[(x, y)] = new Cell { x = x, y = y, open = false };
    }

    // -----------------------------------------------------------------
    // 2. Generate the corridors (backtracker with direction bias + joins)
    // -----------------------------------------------------------------
    public Dictionary<(int, int), Cell> GenerateCorridors()
    {
        if (generated) return grid; // same instance already generated a maze; reuse it

        CreateGrid();
        rng = seed != 0 ? new System.Random(seed) : new System.Random();

        // Initial direction order: E, S, W, N (same as the original)
        var dirs = new List<Vector2Int>
        {
            new(1, 0),
            new(0, 1),
            new(-1, 0),
            new(0, -1),
        };

        var origin = At(0, 0);
        var walker = new Stack<Cell>();
        walker.Push(origin);
        var head = origin;

        // Step counter: how many more steps the current preferred direction
        // stays in effect before a new one is picked. Not related to real time.
        int stepsUntilRedirect = 0;

        while (walker.Count > 0)
        {
            // --- Step 1: refresh the preferred direction, if due ---
            stepsUntilRedirect--;
            if (stepsUntilRedirect < 0)
            {
                int idx = 1 + rng.Next(0, 3); // 1..3, never index 0
                var d = dirs[idx];
                dirs.RemoveAt(idx);
                dirs.Insert(0, d);
                stepsUntilRedirect = minStepsBeforeRedirect + rng.Next(0, 2); // 3 or 4
            }

            // --- Step 2: look for a neighbor to advance into ---
            Cell next = null;
            foreach (var dir in dirs)
            {
                var t = At(head.x + dir.x, head.y + dir.y);
                if (t == null) continue;

                next = t;

                if (!t.open)
                {
                    int walls = 0, wx = 0, wy = 0;
                    foreach (var d2 in dirs)
                    {
                        var n = At(t.x + d2.x, t.y + d2.y);
                        if (n == null || !n.open)
                        {
                            walls++;
                            wx += d2.x * d2.x;
                            wy += d2.y * d2.y;
                        }
                    }

                    if (walls < 3)
                    {
                        // Shortcut between two paths: only if 2 walls aligned on one axis
                        if (rng.NextDouble() < joinPathProbability && walls == 2 && (wx == 0 || wy == 0))
                            t.open = true;

                        next = null; // not used to keep advancing right now
                    }
                }
                else
                {
                    next = null; // already corridor, not a candidate
                }

                if (next != null) break;
            }

            // --- Step 3: advance or backtrack ---
            if (next == null)
            {
                head = walker.Pop();
                stepsUntilRedirect = 0; // force a redirect roll on the next iteration
            }
            else
            {
                next.open = true;
                walker.Push(next);
                head = next;
            }
        }

        // --- Compute open sides for each cell ---
        foreach (var c in grid.Values)
        {
            if (!c.open) continue;
            c.E = At(c.x + 1, c.y)?.open ?? false;
            c.W = At(c.x - 1, c.y)?.open ?? false;
            c.S = At(c.x, c.y + 1)?.open ?? false;
            c.N = At(c.x, c.y - 1)?.open ?? false;
        }

        ComputeDistances(origin);

        generated = true;
        return grid;
    }

    // -----------------------------------------------------------------
    // 3. BFS distance in steps from the entrance, following only open
    //    corridor sides. Everything door/room placement depends on this.
    // -----------------------------------------------------------------
    private void ComputeDistances(Cell origin)
    {
        origin.dist = 0;
        var queue = new Queue<Cell>();
        queue.Enqueue(origin);

        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            foreach (var n in Neighbors(c))
            {
                if (n.dist > c.dist + 1)
                {
                    n.dist = c.dist + 1;
                    queue.Enqueue(n);
                }
            }
        }
    }

    private IEnumerable<Cell> Neighbors(Cell c)
    {
        if (c.E) yield return At(c.x + 1, c.y);
        if (c.W) yield return At(c.x - 1, c.y);
        if (c.S) yield return At(c.x, c.y + 1);
        if (c.N) yield return At(c.x, c.y - 1);
    }
}
