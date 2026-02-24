using UnityEngine;
using System.Collections;

public class TeleportReceiver : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(ApplyTeleportWhenReady());
    }

    private IEnumerator ApplyTeleportWhenReady()
    {
        if (!TeleportData.useTeleportPosition)
            yield break;

        // Wait until PlayerMovement exists
        PlayerMovement player = null;
        while (player == null)
        {
            player = FindFirstObjectByType<PlayerMovement>();
            yield return null;
        }

        // Disable controller before teleport
        var controller = player.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        // Apply teleport
        player.transform.position = TeleportData.spawnPosition;

        // Re-enable controller
        if (controller != null)
            controller.enabled = true;

        // Clear teleport data
        TeleportData.Clear();
    }
}
