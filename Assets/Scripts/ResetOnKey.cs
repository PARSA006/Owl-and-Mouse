using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetOnKey : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "Scene1"; // your starting scene

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Clear all saved data
            SaveManager.ResetGame();

            // Reset teleport data if you're using it
            TeleportData.Clear();

            // Load the starting scene
            SceneManager.LoadScene(firstSceneName);
        }
    }
}
