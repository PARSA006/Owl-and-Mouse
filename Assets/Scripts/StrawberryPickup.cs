using System.Collections;
using UnityEngine;

public class StrawberryPickup : MonoBehaviour
{
    [SerializeField] private string pickupID;
    [SerializeField] private int amount = 1;

    public string PickupID => pickupID;

    private bool collected = false;

    private IEnumerator Start()
    {
        Debug.Log("STRAWBERRY START: checking pickup " + pickupID);

        // Wait TWO frames:
        // 1. Scene loads
        // 2. Checkpoint.RestoreSnapshot() runs
        yield return null;
        yield return null;

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
        if (inv == null) return;

        collected = true;

        inv.AddStrawberries(amount);
        SaveManager.MarkPickupCollected(pickupID);

        Destroy(gameObject);
    }
}
