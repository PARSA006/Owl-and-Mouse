using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Handles player death, scene reloads, and restoring checkpoint data.
public class PlayerRespawn : MonoBehaviour
{
    // Singleton instance so other scripts can call RespawnPlayer()
    public static PlayerRespawn Instance;

    // Tells other systems (PlayerMovement, Inventory, etc.)
    // whether we are restoring from a checkpoint.
    public static bool restoredFromCheckpoint = false;

    private void Awake()
    {
        // Set up singleton
        Instance = this;

        // Keep this object alive across scene loads
        DontDestroyOnLoad(gameObject);
    }

    // Called when the player dies (e.g., by hazards or enemies)
    public void RespawnPlayer()
    {
        Debug.Log("RESPAWN: Player respawned");

        // Mark that we are restoring from a checkpoint
        restoredFromCheckpoint = true;

        string sceneToLoad;

        // If a checkpoint exists, load the scene stored in SaveManager
        if (Checkpoint.lastSnapshot != null)
        {
            sceneToLoad = SaveManager.LoadSceneName();
            Debug.Log("RESPAWN: Loading checkpoint scene: " + sceneToLoad);
        }
        else
        {
            // No checkpoint → reload current scene
            sceneToLoad = SceneManager.GetActiveScene().name;
            Debug.Log("RESPAWN: No checkpoint found, reloading current scene: " + sceneToLoad);
        }

        // Load the scene
        SceneManager.LoadScene(sceneToLoad);

        // Wait for scene to load, then restore checkpoint data
        StartCoroutine(DelayedRestore());
    }

    // Waits until PlayerMovement exists in the new scene,
    // then restores checkpoint data safely.
    private IEnumerator DelayedRestore()
    {
        Debug.Log("RESPAWN: RestoreStarted");

        PlayerMovement pm = null;

        // Wait until PlayerMovement is found in the scene
        while (pm == null)
        {
            pm = Object.FindFirstObjectByType<PlayerMovement>();
            Debug.Log("RESPAWN: Searching for PlayerMovement... found = " + pm);
            yield return null;
        }

        Debug.Log("RESPAWN: Calling RestoreSnapshotTo()...");

        // ⭐ CRITICAL FIX ⭐
        // Disable CharacterController so teleporting doesn't snap back
        var controller = pm.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            Debug.Log("RESPAWN: CharacterController disabled for restore");
        }

        try
        {
            // Restore player + enemies + traps + pickups
            Checkpoint.RestoreSnapshotTo(pm);
            Debug.Log("RESPAWN: RestoreSnapshotTo finished");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("RESPAWN: RestoreSnapshotTo threw exception: " + ex.Message);
        }

        // Re-enable CharacterController AFTER teleporting
        if (controller != null)
        {
            controller.enabled = true;
            Debug.Log("RESPAWN: CharacterController re-enabled after restore");
        }

        // Reset flag so normal save/load works again
        restoredFromCheckpoint = false;
    }
}
