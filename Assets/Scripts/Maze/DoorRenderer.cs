using UnityEngine;

/// <summary>
/// Draws the doors produced by DoorGenerator on top of the corridor tiles
/// drawn by MazeRenderer. Kept separate from generation: this component
/// only reads data and renders it.
/// </summary>
[RequireComponent(typeof(MazeGenerator))]
[RequireComponent(typeof(DoorGenerator))]
public class DoorRenderer : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private DoorTileSet doorTileSet;
    [SerializeField] private StairsTileSet stairsTileSet;

    [Header("Layout")]
    [SerializeField] private float tileSize = 1f; // must match MazeRenderer's tileSize
    [SerializeField] private int sortingOrder = 1; // above the corridor tiles

    private MazeGenerator generator;
    private DoorGenerator doorGenerator;

    void Start()
    {
        generator = GetComponent<MazeGenerator>();
        doorGenerator = GetComponent<DoorGenerator>();

        var grid = generator.GenerateCorridors(); // no-op if MazeRenderer already generated it
        var doors = doorGenerator.GenerateDoors(grid);
        Draw(doors);
    }

    private void Draw(System.Collections.Generic.Dictionary<(int, int), DoorGenerator.DoorCell> doors)
    {
        foreach (var d in doors.Values)
        {
            Sprite sprite = GetSprite(d.doorPosition, d.content);
            if (sprite == null)
            {
                Debug.LogWarning($"No sprite found for door at ({d.x},{d.y}) position={d.doorPosition} content={d.content}");
                continue;
            }

            // y grows downward in the original game -> flipped for Unity (up = +)
            var cellPos = new Vector3(d.x * tileSize, -d.y * tileSize, 0);
            var offset = DoorOffsets.Get(d.doorPosition) * tileSize;
            var pos = cellPos + new Vector3(offset.x, offset.y, 0);

            GameObject doorObj = new($"door_{d.x}_{d.y}");
            doorObj.transform.SetParent(transform);
            doorObj.transform.position = pos;

            SpriteRenderer sr = doorObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
        }
    }

    // Rooms come from DoorTileSet (position + open/closed), stairs come from
    // the separate StairsTileSet (position + away/toward).
    private Sprite GetSprite(DoorPosition position, DoorGenerator.DoorContent content)
    {
        switch (content)
        {
            case DoorGenerator.DoorContent.OpenRoom:
                return doorTileSet != null ? doorTileSet.GetSprite(position, true) : null;
            case DoorGenerator.DoorContent.LockedRoom:
                return doorTileSet != null ? doorTileSet.GetSprite(position, false) : null;
            case DoorGenerator.DoorContent.StairsAway:
                return stairsTileSet != null ? stairsTileSet.GetSprite(position, StairsDirection.Away) : null;
            case DoorGenerator.DoorContent.StairsToward:
                return stairsTileSet != null ? stairsTileSet.GetSprite(position, StairsDirection.Toward) : null;
            default:
                return null;
        }
    }
}
