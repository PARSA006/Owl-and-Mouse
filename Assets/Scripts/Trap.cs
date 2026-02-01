using UnityEngine;
using System.Collections;

public class Trap : MonoBehaviour
{
    public bool triggered = false;
    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        Debug.Log("TRAP: OnEnable called — disabling collider until restore finishes");
        StartCoroutine(EnableAfterRestore());
    }

    private IEnumerator EnableAfterRestore()
    {
        if (col != null)
        {
            col.enabled = false;

            while (PlayerRespawn.restoredFromCheckpoint)
                yield return null;

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
        if (triggered)
            return;

        if (other.transform.root.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("TRAP: Player entered trap — triggering respawn");
            PlayerRespawn.Instance.RespawnPlayer();
        }
    }

    public void SetTriggered(bool value)
    {
        triggered = value;
        Debug.Log("TRAP: Triggered state set to " + value);
    }
}
