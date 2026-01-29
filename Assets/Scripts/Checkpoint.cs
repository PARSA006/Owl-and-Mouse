using UnityEngine;

public static class Checkpoint
{
    public static CheckpointSnapshot lastSnapshot;

    public static void SetSnapshot(CheckpointSnapshot snapshot)
    {
        lastSnapshot = snapshot;
    }

    public static void ClearSnapshot()
    {
        lastSnapshot = null;
    }

    public static void RestoreSnapshot()
    {
        if (lastSnapshot == null)
            return;

        // -------------------------
        // RESTORE INVENTORY
        // -------------------------
        var inv = PlayerMovement.Instance.GetComponent<PlayerInventory>();
        if (inv != null)
            inv.strawberries = lastSnapshot.strawberryCount;

        // -------------------------
        // RESTORE ENEMIES
        // -------------------------
        var enemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length && i < lastSnapshot.enemies.Count; i++)
        {
            enemies[i].RestoreSnapshot(lastSnapshot.enemies[i]);
        }

        // -------------------------
        // RESTORE TRAPS
        // -------------------------
        var traps = Object.FindObjectsByType<Trap>(FindObjectsSortMode.None);
        for (int i = 0; i < traps.Length && i < lastSnapshot.traps.Count; i++)
        {
            traps[i].RestoreSnapshot(lastSnapshot.traps[i]);
        }

        // -------------------------
        // RESTORE PICKUPS
        // -------------------------
        var pickups = Object.FindObjectsByType<StrawberryPickup>(FindObjectsSortMode.None);

        foreach (var pickup in pickups)
        {
            bool wasCollectedAtCheckpoint = lastSnapshot.collectedPickups.Contains(pickup.PickupID);

            if (wasCollectedAtCheckpoint)
            {
                SaveManager.MarkPickupCollected(pickup.PickupID);
                Object.Destroy(pickup.gameObject);
            }
            else
            {
                PlayerPrefs.DeleteKey("pickup_" + pickup.PickupID);
            }
        }

        PlayerPrefs.Save();
    }
}
