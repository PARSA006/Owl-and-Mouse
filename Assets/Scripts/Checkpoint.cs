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

        // PLAYER POSITION
        Debug.Log("RESTORE: Moving player to saved position: " + lastSnapshot.playerPosition);
        player.transform.position = lastSnapshot.playerPosition;
        Debug.Log("RESTORE: Player now at position: " + player.transform.position);

        // INVENTORY
        var inv = Object.FindFirstObjectByType<PlayerInventory>();
        if (inv != null)
        {
            inv.strawberries = lastSnapshot.strawberryCount;
            Debug.Log("RESTORE: Strawberry count restored to " + inv.strawberries);
        }
        else
        {
            Debug.LogWarning("RESTORE: PlayerInventory not found");
        }

        // ENEMIES
        var allEnemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);
        for (int i = 0; i < allEnemies.Length && i < lastSnapshot.enemies.Count; i++)
        {
            allEnemies[i].RestoreSnapshot(lastSnapshot.enemies[i]);
            Debug.Log("RESTORE: Enemy " + i + " restored");
        }

        // TRAPS
        var allTraps = Object.FindObjectsByType<Trap>(FindObjectsSortMode.None);
        for (int i = 0; i < allTraps.Length && i < lastSnapshot.traps.Count; i++)
        {
            allTraps[i].SetTriggered(lastSnapshot.traps[i].triggered);
            Debug.Log("RESTORE: Trap " + i + " triggered state = " + lastSnapshot.traps[i].triggered);
        }

        // PICKUPS
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
