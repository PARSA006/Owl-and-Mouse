using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResetter : MonoBehaviour
{
    public string firstSceneName = "Scene1"; // change to your starting scene

    public void ResetGame()
    {
        SaveManager.ResetGame();
        SceneManager.LoadScene(firstSceneName);
    }
}
