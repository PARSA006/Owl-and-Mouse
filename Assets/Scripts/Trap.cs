using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private GameObject visual; // spikes, blades, etc.

    // This is the state we care about
    public bool triggered { get; private set; } = false;

    private void Reset()
    {
        // Auto-assign collider if not set
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Example: kill player
        PlayerRespawn.RespawnPlayer();

        // Example: disable collider so it doesn't trigger again
        if (triggerCollider != null)
            triggerCollider.enabled = false;

        // Optional: play animation, particles, etc.
    }

    // Called when restoring from snapshot
    public void RestoreSnapshot(TrapSnapshot snap)
    {
        triggered = snap.triggered;

        // Re-apply state to components
        if (triggerCollider != null)
            triggerCollider.enabled = !triggered;

        if (visual != null)
            visual.SetActive(true); // or change based on state if needed
    }
}
