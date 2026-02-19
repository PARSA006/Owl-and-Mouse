using System.Collections;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int strawberries { get; set; } = 0;

    private IEnumerator Start()
    {
        Debug.Log("INVENTORY START: strawberries = " + strawberries +
          " | restoredFromCheckpoint = " + PlayerRespawn.restoredFromCheckpoint);

        // Wait one frame so Checkpoint.RestoreSnapshot() can run first
        yield return null;

        // Only load from SaveManager if we are NOT restoring from a checkpoint
        if (!PlayerRespawn.restoredFromCheckpoint)
        {
            strawberries = SaveManager.HasSave()
                ? SaveManager.LoadStrawberries()
                : 0;
        }

        // ⭐ IMPORTANT: Update HUD AFTER loading saved strawberries
        if (StrawberryHUD.Instance != null)
            StrawberryHUD.Instance.SetCollected(strawberries);
    }



    public void AddStrawberries(int amount)
    {
        strawberries += amount;
    }

    public void SaveInventory()
    {
        SaveManager.SavePlayer(transform.position, strawberries);
    }
}
