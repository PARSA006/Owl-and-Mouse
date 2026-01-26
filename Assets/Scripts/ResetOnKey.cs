using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetOnKey : MonoBehaviour
{
    public string firstSceneName = "Scene1"; // change to your starting scene

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SaveManager.ResetGame();
            SceneManager.LoadScene(firstSceneName);
        }
    }
}
