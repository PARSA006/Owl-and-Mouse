using UnityEngine;
using UnityEngine.SceneManagement;

public class nextscene : MonoBehaviour
{
    public string scenename;
    public int requiredStrawberries = 5;

    private PlayerInventory playerInventory;

    private void Start()
    {
        // New Unity API — replaces FindObjectOfType
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerInventory != null && playerInventory.strawberries >= requiredStrawberries)
            {
                SceneManager.LoadScene(scenename);
            }
            else
            {
                Debug.Log("Door is locked! Need more strawberries.");
            }
        }
    }
}
