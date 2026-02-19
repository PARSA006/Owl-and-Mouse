using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrawberryPickup : MonoBehaviour
{
    public static HashSet<string> AllPickupIDs = new HashSet<string>();

    [SerializeField] private string pickupID;
    [SerializeField] private int amount = 1;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;

    public string PickupID => pickupID;

    private bool collected = false;

    private void Awake()
    {
        // Register this pickup ID globally
        AllPickupIDs.Add(pickupID);

        // Ensure the GameObject name matches the pickupID
        gameObject.name = pickupID;

    }


    private IEnumerator Start()
    {
        // Wait until checkpoint restore is done
        for (int i = 0; i < 5; i++)
            yield return null;

        // If the checkpoint snapshot says this was collected, destroy it
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

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);

        if (StrawberryHUD.Instance != null)
            StrawberryHUD.Instance.AddOne();

        Destroy(gameObject);
    }

}
