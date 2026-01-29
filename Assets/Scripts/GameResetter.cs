using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResetter : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "Scene1"; // your starting scene

    public void ResetGame()
    {
        // Clear all saved game data
        SaveManager.ResetGame();

        // Clear teleport data if you're using it
        TeleportData.Clear();

        // Clear checkpoint snapshot (rewind system)
        Checkpoint.lastSnapshot = null;

        // Load the starting scene
        SceneManager.LoadScene(firstSceneName);
    }
}
