using UnityEngine;
using UnityEngine.SceneManagement;

// This script handles entering a trigger that loads the next scene,
// but ONLY if the player has collected enough strawberries.
// It also stores a teleport position so the player spawns correctly in the next scene.
public class NextScene : MonoBehaviour
{
    // The name of the scene to load when the player enters the trigger.
    [SerializeField] private string sceneName;

    // How many strawberries the player must have to unlock this door.
    [SerializeField] private int requiredStrawberries = 5;

    [Header("Spawn Point in Next Scene")]
    // The position where the player will appear in the next scene.
    // This is saved into TeleportData before loading.
    [SerializeField] private Vector3 teleportSpawnPosition = new Vector3(0f, 1f, 0f);

    private PlayerInventory playerInventory;

    private void Start()
    {
        // Find the player's inventory so we can check strawberry count.
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only react if the object entering the trigger is the player.
        if (!other.CompareTag("Player"))
            return;

        // Check if the player has enough strawberries to unlock the door.
        if (playerInventory != null && playerInventory.strawberries >= requiredStrawberries)
        {
            // ⭐ Save the teleport position so the player spawns correctly in the next scene.
            TeleportData.SetTeleportPosition(teleportSpawnPosition);

            // ⭐ Load the next scene.
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            // Not enough strawberries — door stays locked.
            Debug.Log($"Door is locked! Need {requiredStrawberries} strawberries.");
        }
    }
}
