using UnityEngine;
using UnityEngine.SceneManagement;

// A simple utility script that resets the game when the player presses a key.
// This is mainly useful for debugging or giving the player a quick restart option.
public class ResetOnKey : MonoBehaviour
{
    // The name of the first scene to load when resetting the game.
    // Usually your starting level or main menu.
    [SerializeField] private string firstSceneName = "Scene1";

    private void Update()
    {
        // Listen for the R key being pressed.
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Clear all saved game data (pickups, strawberries, etc.)
            SaveManager.ResetGame();

            // Clear teleport data so the player doesn't spawn at an old location.
            TeleportData.Clear();

            // Load the first scene to restart the game.
            SceneManager.LoadScene(firstSceneName);
        }
    }
}
