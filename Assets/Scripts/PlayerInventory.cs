using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int strawberries = 0;

    private void Start()
    {
        if (SaveManager.HasSave())
        {
            strawberries = SaveManager.LoadStrawberries();
        }
        else
        {
            strawberries = 0; // fresh start
        }
    }

    public void AddStrawberries(int amount)
    {
        strawberries += amount;
    }
}
