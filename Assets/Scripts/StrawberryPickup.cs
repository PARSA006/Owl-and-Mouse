using UnityEngine;

public class StrawberryPickup : MonoBehaviour
{
    public string pickupID; // unique ID for this strawberry
    public int amount = 1;

    private void Start()
    {
        // If already collected, do not respawn
        if (SaveManager.IsPickupCollected(pickupID))
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            inv.AddStrawberries(amount);

            // Mark this pickup as collected
            SaveManager.MarkPickupCollected(pickupID);

            Destroy(gameObject);
        }
    }
}
