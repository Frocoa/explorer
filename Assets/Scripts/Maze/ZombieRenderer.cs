using UnityEngine;

/// <summary>
/// Draws the zombies produced by ZombieGenerator on top of the corridor tiles
/// drawn by MazeRenderer, using ZombieSpotPicker for sub-tile placement.
/// Kept separate from generation: this component only reads data and renders it.
/// </summary>
[RequireComponent(typeof(MazeGenerator))]
[RequireComponent(typeof(ZombieGenerator))]
[RequireComponent(typeof(ZombieSpotPicker))]
public class ZombieRenderer : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("Must expose an \"IsDead\" bool parameter, switching between the alive and dead states.")]
    [SerializeField] private RuntimeAnimatorController zombieController;

    [Header("Layout")]
    [SerializeField] private float tileSize = 1f; // must match MazeRenderer's tileSize
    [SerializeField] private int sortingOrder = 2; // above doors/corridor tiles

    [Header("Debug")]
    [SerializeField] private int seed = 0; // 0 = truly random

    private MazeGenerator generator;
    private ZombieGenerator zombieGenerator;
    private ZombieSpotPicker spotPicker;
    private System.Random rng;

    void Start()
    {
        generator = GetComponent<MazeGenerator>();
        zombieGenerator = GetComponent<ZombieGenerator>();
        spotPicker = GetComponent<ZombieSpotPicker>();
        rng = seed != 0 ? new System.Random(seed) : new System.Random();

        var grid = generator.GenerateCorridors(); // no-op if already generated
        zombieGenerator.Initialize(grid);
        zombieGenerator.PopulateInitial();

        Draw(grid, zombieGenerator.Zombies);
    }

    private void Draw(
        System.Collections.Generic.Dictionary<(int, int), MazeGenerator.Cell> grid,
        System.Collections.Generic.Dictionary<(int, int), ZombieGenerator.ZombieCell> zombies)
    {
        foreach (var z in zombies.Values)
        {
            if (z.alive == 0 && z.killed == 0) continue;

            var cell = grid[(z.x, z.y)];
            var spots = spotPicker.GetSpots(z.alive, z.killed, cell.N, cell.S, cell.E, cell.W, rng);

            // y grows downward in the original game -> flipped for Unity (up = +)
            var cellPos = new Vector3(z.x * tileSize, -z.y * tileSize, 0);

            foreach (var spot in spots)
            {
                if (zombieController == null)
                {
                    Debug.LogWarning("ZombieRenderer has no zombieController assigned.");
                    continue;
                }

                // localPos: 0..1, origin top-left, y grows downward -> convert to
                // a centered world offset, flipping y to match Unity's up = +.
                var offset = new Vector3(
                    (spot.localPos.x - 0.5f) * tileSize,
                    -(spot.localPos.y - 0.5f) * tileSize,
                    0);
                var pos = cellPos + offset;

                GameObject zombieObj = new($"zombie_{z.x}_{z.y}_{(spot.isDead ? "dead" : "alive")}");
                zombieObj.transform.SetParent(transform);
                zombieObj.transform.position = pos;

                SpriteRenderer sr = zombieObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = sortingOrder + (spot.isDead ? 0 : 1); // alive drawn above dead

                Animator animator = zombieObj.AddComponent<Animator>();
                animator.runtimeAnimatorController = zombieController;
                animator.SetBool("isDead", spot.isDead);
            }
        }
    }
}
