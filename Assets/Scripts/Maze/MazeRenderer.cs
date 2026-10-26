using UnityEngine;

/// <summary>
/// Draws a maze grid produced by MazeGenerator using MazeTileSet sprites.
/// Kept separate from generation: this component only reads data and renders it.
/// </summary>
[RequireComponent(typeof(MazeGenerator))]
public class MazeRenderer : MonoBehaviour
{
    [SerializeField] private MazeTileSet tileSet;
    [Tooltip("Always used for the entrance cell (0,0), instead of a tileSet lookup.")]
    [SerializeField] private Sprite exitSprite;

    [Header("Layout")]
    [SerializeField] private float tileSize = 1f; // world-unit spacing between tiles

    private MazeGenerator generator;

    void Start()
    {
        generator = GetComponent<MazeGenerator>();
        var grid = generator.GenerateCorridors();
        Draw(grid);
    }

    private void Draw(System.Collections.Generic.Dictionary<(int, int), MazeGenerator.Cell> grid)
    {
        foreach (var c in grid.Values)
        {
            if (!c.open) continue;

            // The entrance is always the exit sprite, regardless of its connections.
            bool isEntrance = c.x == 0 && c.y == 0;

            // GetSprite takes (N, S, E, W)
            Sprite tileSprite = isEntrance ? exitSprite : tileSet.GetSprite(c.N, c.S, c.E, c.W);
            if (tileSprite == null)
            {
                Debug.LogWarning($"No sprite found for N={c.N} S={c.S} E={c.E} W={c.W} at ({c.x},{c.y})");
                continue;
            }

            // y grows downward in the original game -> flipped for Unity (up = +)
            var pos = new Vector3(c.x * tileSize, -c.y * tileSize, 1);

            GameObject tileObj = new($"tile_{c.x}_{c.y}");
            tileObj.transform.SetParent(transform);
            tileObj.transform.position = pos;

            SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
            sr.sprite = tileSprite;
        }
    }
}
