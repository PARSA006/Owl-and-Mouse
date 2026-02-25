using UnityEngine;
using System.Collections;

// Ensures the player spawns at a specific position when a scene loads.
// This is useful for scenes that have a fixed starting point (e.g., level entrances).
public class SceneStartSpawn : MonoBehaviour
{
    // The position where the player should appear when this scene starts.
    [SerializeField] private Transform startPosition;

    private void Start()
    {
        // Use a coroutine so we can wait one frame before moving the player.
        StartCoroutine(SpawnWhenReady());
    }

    private IEnumerator SpawnWhenReady()
    {
        // Wait 1 frame so the CharacterController has time to initialize.
        // If we teleport the player too early, the controller may snap them back.
        yield return null;

        // Find the PlayerMovement instance in the scene.
        var player = FindFirstObjectByType<PlayerMovement>();
        if (player == null)
            yield break; // No player found → nothing to do.

        // Get the CharacterController so we can temporarily disable it.
        var controller = player.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false; // Prevents teleport snapping.

        // Teleport the player to the scene's designated spawn point.
        player.transform.position = startPosition.position;

        // Re-enable the CharacterController after teleporting.
        if (controller != null)
            controller.enabled = true;
    }
}
