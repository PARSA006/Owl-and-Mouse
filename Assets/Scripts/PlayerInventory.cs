using System.Collections;
using UnityEngine;

// Handles the player's strawberry inventory.
// Works together with SaveManager, Checkpoint, and the HUD.
public class PlayerInventory : MonoBehaviour
{
    // The player's current strawberry count.
    // Using a property so it can be read/written cleanly.
    public int strawberries { get; set; } = 0;

    private IEnumerator Start()
    {
        Debug.Log("INVENTORY START: strawberries = " + strawberries +
          " | restoredFromCheckpoint = " + PlayerRespawn.restoredFromCheckpoint);

        // Wait one frame so Checkpoint.RestoreSnapshot() can run first.
        // This ensures checkpoint data overrides SaveManager data.
        yield return null;

        // Only load saved strawberries if we are NOT restoring from a checkpoint.
        // Checkpoints take priority over SaveManager saves.
        if (!PlayerRespawn.restoredFromCheckpoint)
        {
            strawberries = SaveManager.HasSave()
                ? SaveManager.LoadStrawberries()  // Load saved value
                : 0;                              // No save → start at 0
        }

        // Update the HUD after loading the correct strawberry count.
        if (StrawberryHUD.Instance != null)
            StrawberryHUD.Instance.SetCollected(strawberries);
    }

    // Adds strawberries to the player's inventory.
    public void AddStrawberries(int amount)
    {
        strawberries += amount;
    }

    // Saves the player's inventory and position to disk.
    public void SaveInventory()
    {
        SaveManager.SavePlayer(transform.position, strawberries);
    }
}
