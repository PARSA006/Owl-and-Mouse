using UnityEngine;
using UnityEngine.SceneManagement;

public class nextscene : MonoBehaviour
{
    public string scenename;
    public int requiredStrawberries = 5;

    // The fixed spawn point in the next scene
    public Vector3 teleportSpawnPosition = new Vector3(0f, 1f, 0f);

    private PlayerInventory playerInventory;

    private void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerInventory != null && playerInventory.strawberries >= requiredStrawberries)
            {
                // Tell the next scene to use the teleport spawn
                TeleportData.spawnPosition = teleportSpawnPosition;
                TeleportData.useTeleportPosition = true;

                SceneManager.LoadScene(scenename);
            }
            else
            {
                Debug.Log("Door is locked! Need more strawberries.");
            }
        }
    }
}
