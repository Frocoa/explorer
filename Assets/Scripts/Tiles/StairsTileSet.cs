using System;
using UnityEngine;

/// <summary>
/// Which way a staircase leads relative to the entrance floor.
/// Away = deeper into the ruin (red frame), Toward = back to the entrance (blue frame).
/// Stairs are never locked, so there's no "closed" state here.
/// </summary>
public enum StairsDirection
{
    Away,
    Toward
}

[Serializable] public struct StairsTileData
{
    [Tooltip("Descriptive Name")]
    public string label;

    [Header("Door Slot")]
    public DoorPosition position;
    public StairsDirection direction;

    [Header("Tile Sprite")]
    public Sprite sprite;

    public bool Matches(DoorPosition position, StairsDirection direction)
    {
        return this.position == position && this.direction == direction;
    }
}

[CreateAssetMenu(fileName = "NewStairsSet", menuName = "Maze/Stairs")]
public class StairsTileSet : ScriptableObject
{
    [SerializeField] private StairsTileData[] tiles;

    public Sprite GetSprite(DoorPosition position, StairsDirection direction)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].Matches(position, direction))
            {
                return tiles[i].sprite;
            }
        }

        Debug.LogWarning($"No se encontró un sprite para la escalera: Position:{position}, Direction:{direction}");
        return null;
    }
}
