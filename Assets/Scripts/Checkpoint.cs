using UnityEngine;
using System.Collections;

// A static class that stores and restores checkpoint data.
// It holds the last saved snapshot and can apply it to the player and world.
public static class Checkpoint
{
    // The most recent saved checkpoint snapshot.
    public static CheckpointSnapshot lastSnapshot;

    // Saves a snapshot into memory.
    public static void SetSnapshot(CheckpointSnapshot snapshot)
    {
        lastSnapshot = snapshot;
        Debug.Log("CHECKPOINT: Snapshot saved");
    }

    // Clears the saved snapshot (used when starting a new game).
    public static void ClearSnapshot()
    {
        lastSnapshot = null;
        Debug.Log("CHECKPOINT: Snapshot cleared");
    }

    // Restores the saved snapshot to the player and world.
    public static void RestoreSnapshotTo(PlayerMovement player)
    {
        // If no snapshot exists, do nothing.
        if (lastSnapshot == null)
        {
            Debug.Log("RESTORE SNAPSHOT: No snapshot found");
            return;
        }

        Debug.Log("RESTORE SNAPSHOT: Applying snapshot...");

        // -------------------------
        // PLAYER POSITION
        // -------------------------
        // Move the player to the saved position.
        Debug.Log("RESTORE: Moving player to saved position: " + lastSnapshot.playerPosition);
        player.transform.position = lastSnapshot.playerPosition;

        // -------------------------
        // INVENTORY
        // -------------------------
        // Find the player's inventory and restore strawberry count.
        var inv = Object.FindFirstObjectByType<PlayerInventory>();
        if (inv != null)
        {
            inv.strawberries = lastSnapshot.strawberryCount;
            Debug.Log("RESTORE: Strawberry count restored to " + inv.strawberries);

            // Update the HUD so the UI matches the restored inventory.
            if (StrawberryHUD.Instance != null)
                StrawberryHUD.Instance.SetCollected(inv.strawberries);
        }

        // -------------------------
        // ENEMIES (UPDATED FOR ZONES)
        // -------------------------
        // Find all enemies and all patrol zones currently in the scene.
        var allEnemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);
        var allZones = Object.FindObjectsByType<PatrolZone>(FindObjectsSortMode.None);

        // Restore each enemy's saved state.
        for (int i = 0; i < allEnemies.Length && i < lastSnapshot.enemies.Count; i++)
        {
            var enemy = allEnemies[i];
            var snap = lastSnapshot.enemies[i];

            // Restore basic enemy data (position, state, etc.)
            enemy.RestoreSnapshot(snap);

            // Restore the patrol zone the enemy belonged to.
            foreach (var zone in allZones)
            {
                if (zone.zoneIndex == snap.zoneIndex)
                {
                    enemy.currentZoneIndex = snap.zoneIndex;
                    enemy.currentPatrolPoints = zone.patrolPoints;
                    break;
                }
            }

            Debug.Log("RESTORE: Enemy " + i + " restored to zone " + snap.zoneIndex);
        }

        // -------------------------
        // TRAPS
        // -------------------------
        // Restore trap states (triggered or not).
        var allTraps = Object.FindObjectsByType<Trap>(FindObjectsSortMode.None);
        for (int i = 0; i < allTraps.Length && i < lastSnapshot.traps.Count; i++)
        {
            allTraps[i].SetTriggered(lastSnapshot.traps[i].triggered);
            Debug.Log("RESTORE: Trap " + i + " triggered state = " + lastSnapshot.traps[i].triggered);
        }

        // -------------------------
        // PICKUPS
        // -------------------------
        // Clear all pickup data so we can reapply the saved ones.
        SaveManager.ClearAllCollectedPickups();

        // Mark all pickups that were collected at the time of the snapshot.
        foreach (string id in lastSnapshot.collectedPickups)
        {
            SaveManager.MarkPickupCollected(id);
            Debug.Log("RESTORE: Pickup marked collected: " + id);
        }

        Debug.Log("RESTORE SNAPSHOT: Complete");

        // Optional: verify the player's position after physics settles.
        player.StartCoroutine(VerifyPosition(player));
    }

    // Waits 0.1 seconds and logs the player's position.
    // This helps debug cases where physics or CharacterController snaps the player.
    private static IEnumerator VerifyPosition(PlayerMovement player)
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("VERIFY: Player position after 0.1s = " + player.transform.position);
    }
}
