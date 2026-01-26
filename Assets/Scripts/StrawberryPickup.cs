using UnityEngine;

public class StrawberryPickup : MonoBehaviour
{
    public int amount = 1; // how many strawberries this one gives

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            inv.AddStrawberries(amount);
            Destroy(gameObject);
        }
    }
}
