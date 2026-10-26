using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Places rooms and stairs on top of a corridor grid produced by MazeGenerator,
/// following MazeMaker::generateRoom(). Handles: which cells get a door, whether
/// it's a room or stairs, whether it's locked, and where on the wall it sits.
/// Pure data generation: sprite lookup lives in DoorRenderer.
/// </summary>
[RequireComponent(typeof(MazeGenerator))]
public class DoorGenerator : MonoBehaviour
{
    [Header("Room placement (MyHordes default values)")]
    [SerializeField] private int roomsToPlace = 10;
    [SerializeField] private int minRoomDistance = 5;   // a cell needs dist > this to host a door
    [SerializeField] private int lockDistance = 10;      // a room with dist > this is locked
    [SerializeField] private int doorSpacing = 4;        // minimum steps between two doors
    [SerializeField] private int closeRoomBudget = 2;    // first N rooms are biased to stay unlocked

    [Header("Stairs")]
    [SerializeField] private bool placeStairsAway = true;
    [Tooltip("Hotel/hospital always require dist > lockDistance for the away-stairs. " +
             "The bunker's original code skips that check for its down-stairs — this is a known " +
             "quirk of the original algorithm, not a design choice. Set false to reproduce it.")]
    [SerializeField] private bool requireLockDistanceForStairsAway = true;

    [Header("Debug")]
    [SerializeField] private int seed = 0; // 0 = truly random

    public enum DoorContent
    {
        None,
        OpenRoom,       // unlocked room
        LockedRoom,     // locked room
        StairsAway,     // takes you further from the entrance floor
        StairsToward,   // brings you back toward the entrance floor
    }

    public class DoorCell
    {
        public int x, y;
        public DoorContent content;
        public DoorPosition doorPosition; // which of the 8 wall slots the door sits in
    }

    private Dictionary<(int, int), MazeGenerator.Cell> grid;
    private Dictionary<(int, int), DoorCell> doors;
    private System.Random rng;

    public Dictionary<(int, int), DoorCell> Doors => doors;

    // -----------------------------------------------------------------
    // Public entry point
    // -----------------------------------------------------------------
    public Dictionary<(int, int), DoorCell> GenerateDoors(Dictionary<(int, int), MazeGenerator.Cell> corridorGrid)
    {
        grid = corridorGrid;
        rng = seed != 0 ? new System.Random(seed) : new System.Random();
        doors = new Dictionary<(int, int), DoorCell>();

        // Only corridor cells can host a door, excluding the entrance's own row/column.
        var candidates = new List<MazeGenerator.Cell>();
        foreach (var c in grid.Values)
            if (c.open && c.x * c.y != 0 && c.dist > minRoomDistance)
                candidates.Add(c);

        var spacing = new Dictionary<(int, int), int>();
        foreach (var c in grid.Values) spacing[(c.x, c.y)] = int.MaxValue;

        int roomsLeft = roomsToPlace;
        int stairsAwayLeft = placeStairsAway ? 1 : 0;
        int closeLeft = closeRoomBudget;

        while ((roomsLeft > 0 || stairsAwayLeft > 0) && candidates.Count > 0)
        {
            // Enforce spacing between doors.
            candidates.RemoveAll(c => spacing[(c.x, c.y)] < doorSpacing);
            if (candidates.Count == 0) break;

            bool placeStairsNow = roomsLeft == 0 && stairsAwayLeft > 0;

            List<MazeGenerator.Cell> pool;
            if (placeStairsNow)
            {
                pool = requireLockDistanceForStairsAway
                    ? candidates.FindAll(c => c.dist > lockDistance)
                    : candidates;

                if (pool.Count == 0) pool = candidates; // fall back rather than stall forever
            }
            else
            {
                pool = PickRoomPool(candidates, roomsLeft, closeLeft);
            }

            var chosen = pool[rng.Next(pool.Count)];
            bool locked = chosen.dist > lockDistance;
            if (!locked) closeLeft--;

            var content = placeStairsNow
                ? DoorContent.StairsAway
                : (locked ? DoorContent.LockedRoom : DoorContent.OpenRoom);
            var position = PickDoorPosition(chosen);

            var door = new DoorCell
            {
                x = chosen.x,
                y = chosen.y,
                content = content,
                doorPosition = position,
            };
            doors[(chosen.x, chosen.y)] = door;

            spacing[(chosen.x, chosen.y)] = 0;
            RecomputeSpacing(spacing);

            if (placeStairsNow) stairsAwayLeft--;
            else roomsLeft--;
        }

        return doors;
    }

    // -----------------------------------------------------------------
    // Distance-targeting for regular rooms (mirrors the original's bias):
    // the first couple of rooms lean toward staying unlocked, the last one
    // leans toward the far end of the floor, everything else is spread
    // roughly evenly across the reachable distances.
    // -----------------------------------------------------------------
    private List<MazeGenerator.Cell> PickRoomPool(List<MazeGenerator.Cell> candidates, int roomsLeft, int closeLeft)
    {
        int maxDist = 0;
        foreach (var c in candidates) if (c.dist > maxDist) maxDist = c.dist;

        int target;
        if (closeLeft > 0 && lockDistance >= minRoomDistance)
            target = rng.Next(minRoomDistance, lockDistance + 1);
        else if (roomsLeft <= 1)
            target = rng.Next(Mathf.Max(minRoomDistance, maxDist - 2), maxDist + 1);
        else
            target = rng.Next(minRoomDistance, maxDist + 1);

        List<MazeGenerator.Cell> pool = null;
        for (int attempt = 0; attempt < 20; attempt++)
        {
            pool = candidates.FindAll(c => c.dist == target);
            if (pool.Count > 0) break;
        }
        return (pool != null && pool.Count > 0) ? pool : candidates;
    }

    // Multi-source BFS: recompute, for every corridor cell, its distance to the
    // nearest placed door. Used only to enforce doorSpacing between doors.
    private void RecomputeSpacing(Dictionary<(int, int), int> spacing)
    {
        var queue = new Queue<MazeGenerator.Cell>();
        foreach (var c in grid.Values)
            if (spacing[(c.x, c.y)] == 0)
                queue.Enqueue(c);

        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            int d = spacing[(c.x, c.y)];
            foreach (var n in Neighbors(c))
            {
                if (spacing[(n.x, n.y)] > d + 1)
                {
                    spacing[(n.x, n.y)] = d + 1;
                    queue.Enqueue(n);
                }
            }
        }
    }

    private IEnumerable<MazeGenerator.Cell> Neighbors(MazeGenerator.Cell c)
    {
        if (c.E) yield return grid[(c.x + 1, c.y)];
        if (c.W) yield return grid[(c.x - 1, c.y)];
        if (c.S) yield return grid[(c.x, c.y + 1)];
        if (c.N) yield return grid[(c.x, c.y - 1)];
    }

    // -----------------------------------------------------------------
    // Door position (which of the 8 wall slots): the door goes on whichever
    // side has no open corridor, using a corner slot on a side that does
    // have one (to avoid overlapping the corridor).
    // -----------------------------------------------------------------
    private DoorPosition PickDoorPosition(MazeGenerator.Cell c)
    {
        var valid = new List<DoorPosition>();

        if (c.E) { valid.Add(DoorPosition.TopRight); valid.Add(DoorPosition.BottomRight); }
        else valid.Add(DoorPosition.Right);

        if (c.W) { valid.Add(DoorPosition.TopLeft); valid.Add(DoorPosition.BottomLeft); }
        else valid.Add(DoorPosition.Left);

        if (!c.N) valid.Add(DoorPosition.Top);
        if (!c.S) valid.Add(DoorPosition.Bottom);

        return valid[rng.Next(valid.Count)];
    }

    public static bool IsOpen(DoorContent content) =>
        content == DoorContent.OpenRoom ||
        content == DoorContent.StairsAway ||
        content == DoorContent.StairsToward; // stairs are never locked in the original
}
