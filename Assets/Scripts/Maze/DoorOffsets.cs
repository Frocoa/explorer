using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Where within a tile each of the 8 door slots sits, as an offset from the
/// cell's center (fraction of tileSize). x: right = positive. y: up = positive
/// (matches the same north-up convention used for corridor tile placement).
/// Starting values only — tune to taste against the actual door sprites.
/// </summary>
public static class DoorOffsets
{
    private static readonly Dictionary<DoorPosition, Vector2> ByPosition = new()
    {
        { DoorPosition.TopLeft,     new Vector2(-0.28f,  0.13f) },
        { DoorPosition.Top,         new Vector2( 0.00f,  0.13f) },
        { DoorPosition.TopRight,    new Vector2( 0.28f,  0.13f) },
        { DoorPosition.Left,        new Vector2(-0.11f,  0.00f) },
        { DoorPosition.Right,       new Vector2( 0.11f,  0.00f) },
        { DoorPosition.BottomLeft,  new Vector2(-0.28f, -0.13f) },
        { DoorPosition.Bottom,      new Vector2( 0.00f, -0.13f) },
        { DoorPosition.BottomRight, new Vector2( 0.28f, -0.13f) },
    };

    public static Vector2 Get(DoorPosition position) => ByPosition[position];
}
