using UnityEngine;
using UnityEngine.SceneManagement;

// A simple utility script that resets the entire game back to its initial state.
// This is typically called from a UI button (e.g., "New Game" or "Reset Game").
public class GameResetter : MonoBehaviour
{
    // The name of the first scene in your game.
    // This is the scene that will be loaded when the game resets.
    [SerializeField] private string firstSceneName = "Scene1";

    // Called when the player chooses to reset the game.
    public void ResetGame()
    {
        // Clears all saved pickup data, trap states, enemy states, etc.
        SaveManager.ResetGame();

        // Clears any teleport data stored between scenes.
        TeleportData.Clear();

        // Clears the last checkpoint snapshot so the game starts fresh.
        Checkpoint.ClearSnapshot();

        // Reloads the first scene of the game.
        SceneManager.LoadScene(firstSceneName);
    }
}
