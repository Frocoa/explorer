using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Places zombies across corridor cells, following MazeMaker::populateMaze().
/// Pure game-state logic: how many zombies live in each cell, how many are
/// alive vs. killed. No rendering.
/// </summary>
[RequireComponent(typeof(MazeGenerator))]
public class ZombieGenerator : MonoBehaviour
{
    [Header("Zombies (MyHordes default values)")]
    [SerializeField] private int initialZombies = 10; // ERuinZombiesInitial, per floor

    [Header("Debug")]
    [SerializeField] private int seed = 0; // 0 = truly random

    public class ZombieCell
    {
        public int x, y;
        public int alive;
        public int killed; // resets every night, used only for the "corpses" visual
    }

    private Dictionary<(int, int), MazeGenerator.Cell> grid;
    private Dictionary<(int, int), ZombieCell> zombies;
    private System.Random rng;

    public Dictionary<(int, int), ZombieCell> Zombies => zombies;

    // -----------------------------------------------------------------
    // Call once, right after MazeGenerator.GenerateCorridors().
    // -----------------------------------------------------------------
    public void Initialize(Dictionary<(int, int), MazeGenerator.Cell> corridorGrid)
    {
        grid = corridorGrid;
        rng = seed != 0 ? new System.Random(seed) : new System.Random();

        zombies = new Dictionary<(int, int), ZombieCell>();
        foreach (var c in grid.Values)
            if (c.open)
                zombies[(c.x, c.y)] = new ZombieCell { x = c.x, y = c.y, alive = 0, killed = 0 };
    }

    // Matches the call made right after generating the maze: fresh zombies, no reshuffle needed.
    public void PopulateInitial()
    {
        Populate(initialZombies, reposition: false, clearBodies: true);
    }

    // -----------------------------------------------------------------
    // Core algorithm, 1:1 with populateMaze().
    // -----------------------------------------------------------------
    public void Populate(int zedsToAdd, bool reposition, bool clearBodies, HashSet<(int, int)> skip = null)
    {
        if (reposition || clearBodies)
        {
            foreach (var z in zombies.Values)
            {
                if (clearBodies) z.killed = 0;
                if (reposition)
                {
                    zedsToAdd += z.alive;
                    z.alive = 0;
                }
            }
        }

        var candidates = new List<ZombieCell>();
        foreach (var z in zombies.Values)
        {
            if (z.alive >= 4) continue;
            if (skip != null && skip.Contains((z.x, z.y))) continue;
            if (z.x == 0 && z.y == 0) continue; // entrance never gets zombies
            candidates.Add(z);
        }

        Shuffle(candidates);

        while (zedsToAdd > 0 && candidates.Count > 0)
        {
            var current = candidates[^1];
            candidates.RemoveAt(candidates.Count - 1);

            int spawn = rng.Next(1, Mathf.Min(zedsToAdd, 4 - current.alive) + 1);
            current.alive += spawn;
            zedsToAdd -= spawn;
        }
    }

    // Call when the player kills a zombie in cell (x, y).
    public void KillOne(int x, int y)
    {
        if (!zombies.TryGetValue((x, y), out var z) || z.alive <= 0) return;
        z.alive--;
        z.killed++;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
