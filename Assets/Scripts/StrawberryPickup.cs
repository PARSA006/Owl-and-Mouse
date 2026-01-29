using UnityEngine;

public class StrawberryPickup : MonoBehaviour
{
    [SerializeField] private string pickupID;   // unique ID for this strawberry
    [SerializeField] private int amount = 1;

    private bool collected = false;

    private void Start()
    {
        // If already collected, remove it from the scene
        if (SaveManager.IsPickupCollected(pickupID))
        {
            collected = true;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            collected = true;

            inv.AddStrawberries(amount);

            // Mark this pickup as collected
            SaveManager.MarkPickupCollected(pickupID);

            Destroy(gameObject);
        }
    }
}
