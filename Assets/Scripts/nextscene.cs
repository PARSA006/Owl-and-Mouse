using UnityEngine;
using UnityEngine.SceneManagement;

public class nextscene : MonoBehaviour
{
    public string scenename;
    public int requiredStrawberries = 5; // how many needed to unlock
    public PlayerInventory playerInventory; // reference to the player's inventory

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerInventory.strawberries >= requiredStrawberries)
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
