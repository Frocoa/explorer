using System;
using UnityEngine;


[Serializable] public struct TileConnectionData
{
    [Tooltip("Descriptive Name")]
    public string label;

    [Header("Open Connection")]
    public bool north;
    public bool south;
    public bool east;
    public bool west;

    [Header("Tile Sprite")]
    public Sprite sprite;

    public bool Matches(bool n, bool e, bool s, bool w)
    {
        return north == n && east == e && south == s && west == w;
    }
}

[CreateAssetMenu(fileName = "NewMazeTileSet", menuName = "Maze/Bitmask Tile Set")]
public class MazeTileSet : ScriptableObject
{
    [SerializeField] private TileConnectionData[] tiles;

    public Sprite GetSprite(bool north, bool south, bool east, bool west)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].Matches(north, east, south, west))
            {
                return tiles[i].sprite;
            }
        }

        Debug.LogWarning($"No se encontró un sprite para la combinación: N:{north}, E:{east}, S:{south}, W:{west}");
        return null;
    }
}

