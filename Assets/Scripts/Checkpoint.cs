using UnityEngine;
using System.Collections;

public static class Checkpoint
{
    public static CheckpointSnapshot lastSnapshot;

    public static void SetSnapshot(CheckpointSnapshot snapshot)
    {
        lastSnapshot = snapshot;
        Debug.Log("CHECKPOINT: Snapshot saved");
    }

    public static void ClearSnapshot()
    {
        lastSnapshot = null;
        Debug.Log("CHECKPOINT: Snapshot cleared");
    }

    public static void RestoreSnapshotTo(PlayerMovement player)
    {
        if (lastSnapshot == null)
        {
            Debug.Log("RESTORE SNAPSHOT: No snapshot found");
            return;
        }

        Debug.Log("RESTORE SNAPSHOT: Applying snapshot...");

        // -------------------------
        // PLAYER POSITION
        // -------------------------
        Debug.Log("RESTORE: Moving player to saved position: " + lastSnapshot.playerPosition);
        player.transform.position = lastSnapshot.playerPosition;

        // -------------------------
        // INVENTORY
        // -------------------------
        var inv = Object.FindFirstObjectByType<PlayerInventory>();
        if (inv != null)
        {
            inv.strawberries = lastSnapshot.strawberryCount;
            Debug.Log("RESTORE: Strawberry count restored to " + inv.strawberries);

            // ⭐ NEW: Update HUD after restoring inventory
            if (StrawberryHUD.Instance != null)
                StrawberryHUD.Instance.SetCollected(inv.strawberries);
        }

        // -------------------------
        // ENEMIES (UPDATED FOR ZONES)
        // -------------------------
        var allEnemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);
        var allZones = Object.FindObjectsByType<PatrolZone>(FindObjectsSortMode.None);

        for (int i = 0; i < allEnemies.Length && i < lastSnapshot.enemies.Count; i++)
        {
            var enemy = allEnemies[i];
            var snap = lastSnapshot.enemies[i];

            // Restore basic enemy state
            enemy.RestoreSnapshot(snap);

            // Restore patrol zone
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
        var allTraps = Object.FindObjectsByType<Trap>(FindObjectsSortMode.None);
        for (int i = 0; i < allTraps.Length && i < lastSnapshot.traps.Count; i++)
        {
            allTraps[i].SetTriggered(lastSnapshot.traps[i].triggered);
            Debug.Log("RESTORE: Trap " + i + " triggered state = " + lastSnapshot.traps[i].triggered);
        }

        // -------------------------
        // PICKUPS
        // -------------------------
        SaveManager.ClearAllCollectedPickups();

        foreach (string id in lastSnapshot.collectedPickups)
        {
            SaveManager.MarkPickupCollected(id);
            Debug.Log("RESTORE: Pickup marked collected: " + id);
        }

        Debug.Log("RESTORE SNAPSHOT: Complete");

        // Optional: verify after 0.1s
        player.StartCoroutine(VerifyPosition(player));
    }

    private static IEnumerator VerifyPosition(PlayerMovement player)
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("VERIFY: Player position after 0.1s = " + player.transform.position);
    }
}
