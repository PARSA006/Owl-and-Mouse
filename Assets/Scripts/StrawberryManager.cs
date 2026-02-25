using UnityEngine;

// This script sets the total number of strawberries in the level
// and updates the HUD accordingly. It should be placed in each scene
// that contains strawberry pickups.
public class StrawberryManager : MonoBehaviour
{
    private void Start()
    {
        // Set the total number of strawberries once at the start of the scene.
        // StrawberryPickup.AllPickupIDs contains a list of ALL strawberry IDs in the scene.
        if (StrawberryHUD.Instance != null)
            StrawberryHUD.Instance.SetTotal(StrawberryPickup.AllPickupIDs.Count);
    }
}
