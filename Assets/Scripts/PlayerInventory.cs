using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    public int strawberries = 0;

    public UnityEvent<int> OnStrawberryCountChanged;

    private static PlayerInventory instance;

    private void Awake()
    {
        // If an instance already exists, destroy this duplicate
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddStrawberries(int amount)
    {
        strawberries += amount;
        OnStrawberryCountChanged?.Invoke(strawberries);
    }
}
