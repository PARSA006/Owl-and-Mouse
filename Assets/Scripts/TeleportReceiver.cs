using UnityEngine;
using System.Collections;

// Applies a teleport position to the player when a new scene loads,
// but ONLY if TeleportData.useTeleportPosition is true.
// This is used together with NextScene and TeleportData.
public class TeleportReceiver : MonoBehaviour
{
    private void Start()
    {
        // Use a coroutine so we can wait until PlayerMovement exists.
        StartCoroutine(ApplyTeleportWhenReady());
    }

    private IEnumerator ApplyTeleportWhenReady()
    {
        // If no teleport was requested, do nothing.
        if (!TeleportData.useTeleportPosition)
            yield break;

        // Wait until PlayerMovement is present in the scene.
        PlayerMovement player = null;
        while (player == null)
        {
            player = FindFirstObjectByType<PlayerMovement>();
            yield return null; // Wait one frame
        }

        // Disable CharacterController before teleporting.
        // This prevents Unity from snapping the player back to the old position.
        var controller = player.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        // Apply the teleport position.
        player.transform.position = TeleportData.spawnPosition;

        // Re-enable CharacterController after teleport.
        if (controller != null)
            controller.enabled = true;

        // Clear teleport data so future scene loads behave normally.
        TeleportData.Clear();
    }
}
