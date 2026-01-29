using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class PlayerRespawn
{
    public static bool restoredFromCheckpoint = false;

    public static void RespawnPlayer()
    {
        restoredFromCheckpoint = true;

        SceneManager.sceneLoaded += OnSceneLoadedRespawn;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static void OnSceneLoadedRespawn(Scene scene, LoadSceneMode mode)
    {
        PlayerMovement.Instance.StartCoroutine(DelayedRestore());
        SceneManager.sceneLoaded -= OnSceneLoadedRespawn;
    }

    private static IEnumerator DelayedRestore()
    {
        // Wait 1 frame so all pickups, traps, enemies exist
        yield return null;

        if (Checkpoint.lastSnapshot != null)
            Checkpoint.RestoreSnapshot();

        restoredFromCheckpoint = false;
    }
}
