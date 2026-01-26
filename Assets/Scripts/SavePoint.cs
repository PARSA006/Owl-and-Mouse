using UnityEngine;

public class SavePoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inv = other.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                SaveManager.SavePlayer(other.transform.position, inv.strawberries);
                Debug.Log("Game Saved!");
            }
        }
    }
}
