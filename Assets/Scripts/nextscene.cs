using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private int requiredStrawberries = 5;

    [Header("Spawn Point in Next Scene")]
    [SerializeField] private Vector3 teleportSpawnPosition = new Vector3(0f, 1f, 0f);

    private PlayerInventory playerInventory;

    private void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (playerInventory != null && playerInventory.strawberries >= requiredStrawberries)
        {
            TeleportData.SetTeleportPosition(teleportSpawnPosition);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log($"Door is locked! Need {requiredStrawberries} strawberries.");
        }
    }
}
