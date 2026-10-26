using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Tooltip("The same MazeGenerator used by MazeRenderer (drag the Maze GameObject here).")]
    [SerializeField] private MazeGenerator generator;
    [Tooltip("Must match MazeRenderer's tileSize.")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float moveCooldown = 1f;

    private float nextMoveTime;

    void Start()
    {
        // Idempotent: if MazeRenderer already generated this instance's grid,
        // this just returns the existing one. No race condition either way.
        generator.GenerateCorridors();
    }

    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (Time.time < nextMoveTime) return;

        Vector3 dir;
        if (keyboard.upArrowKey.wasPressedThisFrame)
            dir = Vector3.up;
        else if (keyboard.downArrowKey.wasPressedThisFrame)
            dir = Vector3.down;
        else if (keyboard.leftArrowKey.wasPressedThisFrame)
            dir = Vector3.left;
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
            dir = Vector3.right;
        else
            return;

        if (!CanMove(dir)) return;

        transform.Translate(dir * tileSize, Space.World);
        nextMoveTime = Time.time + moveCooldown;
    }

    // Checks whether the maze cell the player currently stands on has an
    // open side in the given world-space direction (up/down/left/right = N/S/E/W).
    private bool CanMove(Vector3 dir)
    {
        int gridX = Mathf.RoundToInt(transform.position.x / tileSize);
        int gridY = Mathf.RoundToInt(-transform.position.y / tileSize);

        generator.Grid.TryGetValue((gridX, gridY), out var cell);
        if (cell == null || !cell.open) return false;

        if (dir == Vector3.up) return cell.N;
        if (dir == Vector3.down) return cell.S;
        if (dir == Vector3.right) return cell.E;
        if (dir == Vector3.left) return cell.W;
        return false;
    }
}
