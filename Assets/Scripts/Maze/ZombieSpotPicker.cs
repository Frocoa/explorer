using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Picks sub-tile positions for the zombie sprites inside one cell, mirroring
/// the client's MapActorZombiesLayer. Purely visual: does not affect gameplay.
/// Call once per cell, when the zombie count for that cell is set. No caching:
/// mazes are never regenerated here, so positions never need to survive a refresh.
/// </summary>
public class ZombieSpotPicker : MonoBehaviour
{
    public struct Spot
    {
        public Vector2 localPos; // 0..1 within the tile, (0,0) = top-left, y grows downward
        public bool isDead;
    }

    private static readonly (Vector2 min, Vector2 max)[] BaseSlots =
    {
        (new Vector2(0.375f, 0.375f), new Vector2(0.425f, 0.625f)), // west-of-center strip
        (new Vector2(0.575f, 0.375f), new Vector2(0.625f, 0.625f)), // east-of-center strip
        (new Vector2(0.375f, 0.375f), new Vector2(0.625f, 0.425f)), // north-of-center strip
        (new Vector2(0.375f, 0.575f), new Vector2(0.625f, 0.625f)), // south-of-center strip
    };

    public List<Spot> GetSpots(int alive, int killed, bool cN, bool cS, bool cE, bool cW, System.Random rng)
    {
        int total = alive + killed;
        var result = new List<Spot>(total);
        if (total <= 0) return result;

        var slots = new List<(Vector2 min, Vector2 max)>(BaseSlots);
        if (cW) slots.Add((new Vector2(0.175f, 0.375f), new Vector2(0.375f, 0.625f)));
        if (cE) slots.Add((new Vector2(0.625f, 0.375f), new Vector2(0.825f, 0.625f)));
        if (cN) slots.Add((new Vector2(0.375f, 0.175f), new Vector2(0.625f, 0.375f)));
        if (cS) slots.Add((new Vector2(0.375f, 0.625f), new Vector2(0.625f, 0.825f)));

        for (int i = 0; i < total; i++)
        {
            var slot = slots[rng.Next(slots.Count)];
            result.Add(new Spot
            {
                localPos = new Vector2(
                    Mathf.Lerp(slot.min.x, slot.max.x, (float)rng.NextDouble()),
                    Mathf.Lerp(slot.min.y, slot.max.y, (float)rng.NextDouble())
                ),
                isDead = i >= alive,
            });
        }

        return result;
    }
}
