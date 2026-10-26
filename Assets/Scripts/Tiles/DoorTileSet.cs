using System;
using UnityEngine;

/// <summary>
/// The 8 wall slots a door can occupy, same numbering as MyHordes'
/// assetDoors(): 1=top-left 2=top 3=top-right 4=left 5=right
/// 6=bottom-left 7=bottom 8=bottom-right. Shared with StairsTileSet.
/// </summary>
public enum DoorPosition
{
    TopLeft,
    Top,
    TopRight,
    Left,
    Right,
    BottomLeft,
    Bottom,
    BottomRight
}

[Serializable] public struct DoorTileData
{
    [Tooltip("Descriptive Name")]
    public string label;

    [Header("Door Slot")]
    public DoorPosition position;
    public bool isOpen;

    [Header("Tile Sprite")]
    public Sprite sprite;

    public bool Matches(DoorPosition position, bool isOpen)
    {
        return this.position == position && this.isOpen == isOpen;
    }
}

[CreateAssetMenu(fileName = "NewDoorSet", menuName = "Maze/Door")]
public class DoorTileSet : ScriptableObject
{
    [SerializeField] private DoorTileData[] tiles;

    public Sprite GetSprite(DoorPosition position, bool isOpen)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].Matches(position, isOpen))
            {
                return tiles[i].sprite;
            }
        }

        Debug.LogWarning($"No se encontró un sprite para la puerta: Position:{position}, Open:{isOpen}");
        return null;
    }
}
