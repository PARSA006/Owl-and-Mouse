using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;


public static class SaveManager
{
    // -------------------------
    // PLAYER POSITION & SCENE
    // -------------------------

    public static void SavePlayer(Vector3 position, int strawberries)
    {
        PlayerPrefs.SetFloat("PlayerX", position.x);
        PlayerPrefs.SetFloat("PlayerY", position.y);
        PlayerPrefs.SetFloat("PlayerZ", position.z);

        PlayerPrefs.SetInt("Strawberries", strawberries);

        string sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedScene", sceneName);

        Debug.Log("SAVE MANAGER: Saving checkpoint in scene: " + sceneName);

        PlayerPrefs.Save();
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey("PlayerX");
    }

    public static Vector3 LoadPlayerPosition()
    {
        float x = PlayerPrefs.GetFloat("PlayerX", 0f);
        float y = PlayerPrefs.GetFloat("PlayerY", 0f);
        float z = PlayerPrefs.GetFloat("PlayerZ", 0f);

        return new Vector3(x, y, z);
    }

    public static int LoadStrawberries()
    {
        return PlayerPrefs.GetInt("Strawberries", 0);
    }

    public static string LoadSceneName()
    {
        string scene = PlayerPrefs.GetString("SavedScene", "");
        Debug.Log("SAVE MANAGER: Loading saved scene: " + scene);
        return scene;
    }

    // -------------------------
    // PICKUP SYSTEM
    // -------------------------

    public static void MarkPickupCollected(string id)
    {
        PlayerPrefs.SetInt("pickup_" + id, 1);
    }

    public static bool IsPickupCollected(string id)
    {
        return PlayerPrefs.GetInt("pickup_" + id, 0) == 1;
    }

    public static void UnmarkPickupCollected(string id)
    {
        PlayerPrefs.DeleteKey("pickup_" + id);
    }

    public static void ClearAllCollectedPickups()
    {
        foreach (string id in StrawberryPickup.AllPickupIDs)
        {
            PlayerPrefs.DeleteKey("pickup_" + id);
        }
    }

    // -------------------------
    // RESET EVERYTHING
    // -------------------------

    public static void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

}
