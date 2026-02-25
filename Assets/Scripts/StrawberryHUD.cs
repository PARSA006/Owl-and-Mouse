using UnityEngine;
using TMPro;

// Handles displaying the player's strawberry count on the HUD.
// Works together with PlayerInventory and SaveManager.
public class StrawberryHUD : MonoBehaviour
{
    // Singleton instance so other scripts can easily update the HUD.
    public static StrawberryHUD Instance;

    // Reference to the UI text element that shows "collected / total".
    [SerializeField] private TextMeshProUGUI counterText;

    private int collected = 0; // How many strawberries the player currently has.
    private int total = 0;     // Total strawberries available in the level.

    private void Awake()
    {
        // Register this HUD as the global instance.
        Instance = this;
    }

    // Sets the total number of strawberries in the level.
    public void SetTotal(int amount)
    {
        total = amount;
        UpdateText();
    }

    // Adds one strawberry to the HUD count.
    // Called when the player picks up a strawberry.
    public void AddOne()
    {
        collected++;
        UpdateText();
    }

    // Updates the UI text to show the current count.
    private void UpdateText()
    {
        counterText.text = collected + " / " + total;
    }

    // Sets the collected amount directly (used when loading saves or checkpoints).
    public void SetCollected(int amount)
    {
        collected = amount;
        UpdateText();
    }
}
