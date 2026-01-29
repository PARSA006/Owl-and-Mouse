using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int strawberries { get; private set; } = 0;

    private void Start()
    {
        // Load saved strawberries if a save exists
        strawberries = SaveManager.HasSave()
            ? SaveManager.LoadStrawberries()
            : 0;
    }

    public void AddStrawberries(int amount)
    {
        strawberries += amount;
    }

    public void SaveInventory()
    {
        // Optional helper if you ever want to save manually
        SaveManager.SavePlayer(transform.position, strawberries);
    }
}
