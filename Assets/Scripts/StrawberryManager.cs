using UnityEngine;

public class StrawberryManager : MonoBehaviour
{
    private void Start()
    {
        // Set total strawberries once at the start
        if (StrawberryHUD.Instance != null)
            StrawberryHUD.Instance.SetTotal(StrawberryPickup.AllPickupIDs.Count);
    }
}
