using UnityEngine;
using TMPro;

public class StrawberryHUD : MonoBehaviour
{
    public static StrawberryHUD Instance;

    [SerializeField] private TextMeshProUGUI counterText;

    private int collected = 0;
    private int total = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void SetTotal(int amount)
    {
        total = amount;
        UpdateText();
    }

    public void AddOne()
    {
        collected++;
        UpdateText();
    }

    private void UpdateText()
    {
        counterText.text = collected + " / " + total;
    }
    public void SetCollected(int amount)
    {
        collected = amount;
        UpdateText();
    }

}
