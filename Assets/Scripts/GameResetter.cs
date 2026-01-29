using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResetter : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "Scene1";

    public void ResetGame()
    {
        SaveManager.ResetGame();
        TeleportData.Clear();
        Checkpoint.ClearSnapshot();
        SceneManager.LoadScene(firstSceneName);
    }
}
