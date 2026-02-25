using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles a single strawberry collectible in the world.
// Each pickup has a unique ID so it can be saved, restored, and prevented from respawning.
public class StrawberryPickup : MonoBehaviour
{
    // A global list of all pickup IDs in the scene.
    // Used by StrawberryManager and SaveManager.
    public static HashSet<string> AllPickupIDs = new HashSet<string>();

    [SerializeField] private string pickupID; // Unique identifier for this pickup
    [SerializeField] private int amount = 1;  // How many strawberries this pickup gives

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;

    // Public read-only access to the ID
    public string PickupID => pickupID;

    private bool collected = false; // Prevents double-collection

    private void Awake()
    {
        // Register this pickup ID globally so the HUD knows the total count.
        AllPickupIDs.Add(pickupID);

        // Ensure the GameObject name matches the pickup ID.
        // This makes GameObject.Find(id) work reliably.
        gameObject.name = pickupID;
    }

    private IEnumerator Start()
    {
        // Wait a few frames so checkpoint restore can finish first.
        // This prevents the pickup from respawning incorrectly.
        for (int i = 0; i < 5; i++)
            yield return null;

        // If SaveManager says this pickup was already collected,
        // destroy it so it doesn't appear again.
        if (SaveManager.IsPickupCollected(pickupID))
        {
            collected = true;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent double-collection
        if (collected) return;

        // Only the player can collect strawberries
        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv == null) return;

        collected = true;

        // Add strawberries to the player's inventory
        inv.AddStrawberries(amount);

        // Play pickup sound
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);

        // Update HUD
        if (StrawberryHUD.Instance != null)
            StrawberryHUD.Instance.AddOne();

        // Remove the pickup from the world
        Destroy(gameObject);
    }
}
