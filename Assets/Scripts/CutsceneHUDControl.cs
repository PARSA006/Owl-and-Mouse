using UnityEngine;

public class CutsceneHUDControl : MonoBehaviour
{
    [SerializeField] private GameObject hudCanvas;   // Assign your HUD Canvas here
    [SerializeField] private float cutsceneDuration = 5f; // Length of your cutscene

    private void Start()
    {
        if (hudCanvas != null)
            hudCanvas.SetActive(false);

        // Re-enable HUD after cutscene
        Invoke(nameof(ShowHUD), cutsceneDuration);
    }

    private void ShowHUD()
    {
        if (hudCanvas != null)
            hudCanvas.SetActive(true);
    }
}
