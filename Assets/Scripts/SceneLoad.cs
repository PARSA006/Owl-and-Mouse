using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("InsideBarrel1(Home) First scene");
    }
    private void Awake()
    {
        SaveManager.DeleteSave();
        PlayerPrefs.DeleteAll();
    }
}
