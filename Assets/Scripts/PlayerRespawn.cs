using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerRespawn
{
    public static bool restoredFromCheckpoint = false;

    public static void RespawnPlayer()
    {
        // 1. Restore player position from SaveManager (your existing system)
        if (SaveManager.HasSave())
        {
            Vector3 savedPos = SaveManager.LoadPlayerPosition();

            var player = Object.FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                var controller = player.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                    player.transform.position = savedPos;
                    controller.enabled = true;
                }
                else
                {
                    player.transform.position = savedPos;
                }
            }
        }
        else
        {
            // No checkpoint → restart the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        // 2. Restore world snapshot (rewind system)
        if (Checkpoint.lastSnapshot != null)
        {
            restoredFromCheckpoint = true;   // ← IMPORTANT
            RestoreSnapshot();
            restoredFromCheckpoint = false;  // ← Reset after restore
        }
    }

    private static void RestoreSnapshot()
    {
        var snap = Checkpoint.lastSnapshot;
        if (snap == null) return;

        // Restore player
        var player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            var controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                player.transform.position = snap.playerPosition;
                controller.enabled = true;
            }
            else
            {
                player.transform.position = snap.playerPosition;
            }
        }

        // Restore enemies
        var allEnemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);

        int enemyCount = Mathf.Min(allEnemies.Length, snap.enemies.Count);
        for (int i = 0; i < enemyCount; i++)
        {
            allEnemies[i].RestoreSnapshot(snap.enemies[i]);
        }

        // Restore traps
        var allTraps = Object.FindObjectsByType<Trap>(FindObjectsSortMode.None);

        int trapCount = Mathf.Min(allTraps.Length, snap.traps.Count);
        for (int i = 0; i < trapCount; i++)
        {
            allTraps[i].RestoreSnapshot(snap.traps[i]);
        }
    }
}
