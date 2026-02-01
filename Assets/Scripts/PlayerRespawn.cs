using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    public static PlayerRespawn Instance;

    public static bool restoredFromCheckpoint = false;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep alive across scene loads
    }

    public void RespawnPlayer()
    {
        Debug.Log("RESPAWN: Player respawned");

        restoredFromCheckpoint = true;

        string sceneToLoad;

        if (Checkpoint.lastSnapshot != null)
        {
            sceneToLoad = SaveManager.LoadSceneName();
            Debug.Log("RESPAWN: Loading checkpoint scene: " + sceneToLoad);
        }
        else
        {
            sceneToLoad = SceneManager.GetActiveScene().name;
            Debug.Log("RESPAWN: No checkpoint found, reloading current scene: " + sceneToLoad);
        }

        SceneManager.LoadScene(sceneToLoad);

        StartCoroutine(DelayedRestore());
    }

    private IEnumerator DelayedRestore()
    {
        Debug.Log("RESPAWN: RestoreStarted");

        PlayerMovement pm = null;

        while (pm == null)
        {
            pm = Object.FindFirstObjectByType<PlayerMovement>();
            Debug.Log("RESPAWN: Searching for PlayerMovement... found = " + pm);
            yield return null;
        }

        Debug.Log("RESPAWN: Calling RestoreSnapshotTo()...");

        // ⭐ CRITICAL FIX ⭐
        // Disable CharacterController so teleporting works without snapping back
        var controller = pm.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            Debug.Log("RESPAWN: CharacterController disabled for restore");
        }

        try
        {
            Checkpoint.RestoreSnapshotTo(pm);
            Debug.Log("RESPAWN: RestoreSnapshotTo finished");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("RESPAWN: RestoreSnapshotTo threw exception: " + ex.Message);
        }

        // Re-enable AFTER teleport
        if (controller != null)
        {
            controller.enabled = true;
            Debug.Log("RESPAWN: CharacterController re-enabled after restore");
        }

        restoredFromCheckpoint = false;
    }
}
