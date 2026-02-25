using UnityEngine;
using System.Collections;

// A trap that kills the player on contact and triggers a respawn.
// It also cooperates with the checkpoint system to avoid firing
// during scene load or checkpoint restoration.
public class Trap : MonoBehaviour
{
    // Whether this trap has already been triggered.
    // Used so traps don't fire multiple times.
    public bool triggered = false;

    private Collider col;

    private void Awake()
    {
        // Cache the collider for enabling/disabling.
        col = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        Debug.Log("TRAP: OnEnable called — disabling collider until restore finishes");

        // During scene load or checkpoint restore, traps should NOT fire.
        // We disable the collider temporarily and re-enable it later.
        StartCoroutine(EnableAfterRestore());
    }

    private IEnumerator EnableAfterRestore()
    {
        if (col != null)
        {
            // Disable collider immediately to prevent accidental triggering.
            col.enabled = false;

            // Wait until the checkpoint restore process is fully complete.
            // PlayerRespawn.restoredFromCheckpoint is true during restore.
            while (PlayerRespawn.restoredFromCheckpoint)
                yield return null;

            // Now it's safe to re-enable the trap.
            col.enabled = true;
            Debug.Log("TRAP: Collider re-enabled after restore");
        }
        else
        {
            Debug.LogWarning("TRAP: Collider not found");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If already triggered, ignore further collisions.
        if (triggered)
            return;

        // Only the player can trigger the trap.
        if (other.transform.root.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("TRAP: Player entered trap — triggering respawn");

            // Tell the respawn system to reload the scene and restore checkpoint.
            PlayerRespawn.Instance.RespawnPlayer();
        }
    }

    // Allows checkpoint restore to set the trap's triggered state.
    public void SetTriggered(bool value)
    {
        triggered = value;
        Debug.Log("TRAP: Triggered state set to " + value);
    }
}
