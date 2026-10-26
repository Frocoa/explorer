using TMPro;
using UnityEngine;

/// <summary>
/// 5-minute oxygen timer. Time stays frozen while the player remains on the
/// starting tile (up to a 30s grace period); it starts counting down as soon
/// as the player moves away, or after the grace period runs out.
/// </summary>
public class OxygenMeter : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text label;
    [SerializeField] private float graceTime = 30f;
    [SerializeField] private float oxygenDuration = 300f; // 5 minutes

    private Vector3 startPosition;
    private float graceElapsed;
    private float oxygenRemaining;
    private bool timerStarted;
    private bool dead;

    void Start()
    {
        startPosition = player.position;
        oxygenRemaining = oxygenDuration;
        UpdateLabel();
    }

    void Update()
    {
        if (dead) return;

        if (!timerStarted)
        {
            if (player.position != startPosition)
            {
                timerStarted = true;
            }
            else
            {
                graceElapsed += Time.deltaTime;
                if (graceElapsed >= graceTime)
                    timerStarted = true;
            }
        }

        if (timerStarted)
        {
            oxygenRemaining -= Time.deltaTime;
            if (oxygenRemaining <= 0f)
            {
                oxygenRemaining = 0f;
                Die();
            }
        }

        UpdateLabel();
    }

    private void UpdateLabel()
    {
        int percent = Mathf.CeilToInt(oxygenRemaining / oxygenDuration * 100f);
        label.text = $"Oxygen: {percent}%";
    }

    private void Die()
    {
        dead = true;
        label.color = Color.red;

        var playerMovement = player.GetComponent<Player>();
        if (playerMovement != null)
            playerMovement.enabled = false;
    }
}
