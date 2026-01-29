using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetOnKey : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "Scene1";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SaveManager.ResetGame();
            TeleportData.Clear();
            SceneManager.LoadScene(firstSceneName);
        }
    }
}
