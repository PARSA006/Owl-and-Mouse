using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

// A static class that handles saving and loading persistent game data.
// Uses PlayerPrefs for simplicity (key-value storage).
public static class SaveManager
{
    // -------------------------
    // PLAYER POSITION & SCENE
    // -------------------------

    // Saves the player's position, strawberry count, and current scene.
    public static void SavePlayer(Vector3 position, int strawberries)
    {
        // Save player position
        PlayerPrefs.SetFloat("PlayerX", position.x);
        PlayerPrefs.SetFloat("PlayerY", position.y);
        PlayerPrefs.SetFloat("PlayerZ", position.z);

        // Save strawberry count
        PlayerPrefs.SetInt("Strawberries", strawberries);

        // Save the current scene name
        string sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedScene", sceneName);

        Debug.Log("SAVE MANAGER: Saving checkpoint in scene: " + sceneName);

        // Write all changes to disk
        PlayerPrefs.Save();
    }

    // Returns true if a save file exists
    public static bool HasSave()
    {
        return PlayerPrefs.HasKey("PlayerX");
    }

    // Loads the player's last saved position
    public static Vector3 LoadPlayerPosition()
    {
        float x = PlayerPrefs.GetFloat("PlayerX", 0f);
        float y = PlayerPrefs.GetFloat("PlayerY", 0f);
        float z = PlayerPrefs.GetFloat("PlayerZ", 0f);

        return new Vector3(x, y, z);
    }

    // Loads the saved strawberry count
    public static int LoadStrawberries()
    {
        return PlayerPrefs.GetInt("Strawberries", 0);
    }

    // Loads the name of the scene stored in the save file
    public static string LoadSceneName()
    {
        string scene = PlayerPrefs.GetString("SavedScene", "");
        Debug.Log("SAVE MANAGER: Loading saved scene: " + scene);
        return scene;
    }

    // -------------------------
    // PICKUP SYSTEM
    // -------------------------

    // Marks a pickup as collected using its unique ID
    public static void MarkPickupCollected(string id)
    {
        PlayerPrefs.SetInt("pickup_" + id, 1);
    }

    // Checks if a pickup has been collected before
    public static bool IsPickupCollected(string id)
    {
        return PlayerPrefs.GetInt("pickup_" + id, 0) == 1;
    }

    // Removes a pickup's collected flag
    public static void UnmarkPickupCollected(string id)
    {
        PlayerPrefs.DeleteKey("pickup_" + id);
    }

    // Clears all pickup data using the list of all pickup IDs
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

    // Completely wipes all saved data (used for New Game or debugging)
    public static void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    // Same as ResetGame(), included for clarity
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
