using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private GameObject visual;

    public bool triggered { get; private set; } = false;

    private void Reset()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        PlayerRespawn.RespawnPlayer();

        if (triggerCollider != null)
            triggerCollider.enabled = false;
    }

    public void RestoreSnapshot(TrapSnapshot snap)
    {
        triggered = snap.triggered;

        if (triggerCollider != null)
            triggerCollider.enabled = !triggered;

        if (visual != null)
            visual.SetActive(true);
    }
}
