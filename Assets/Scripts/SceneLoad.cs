using UnityEngine;
using UnityEngine.SceneManagement;

// This script is placed in a "bootstrap" scene that loads FIRST.
// Its job is to:
// 1. Clear all save data (fresh start)
// 2. Load the real first scene of the game
public class BootstrapLoader : MonoBehaviour
{
    private void Start()
    {
        // Load the actual first scene of the game.
        // This happens immediately after Awake().
        SceneManager.LoadScene("InsideBarrel1(Home) First scene");
    }

    private void Awake()
    {
        // Delete all saved data BEFORE loading the first scene.
        // This ensures the game always starts clean when launched.
        SaveManager.DeleteSave();

        // Extra safety: clear PlayerPrefs entirely.
        PlayerPrefs.DeleteAll();
    }
}
