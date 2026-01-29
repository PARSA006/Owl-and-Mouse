using UnityEngine;

public static class SaveManager
{
    // -------------------------
    // PLAYER SAVE / LOAD
    // -------------------------
    public static void SavePlayer(Vector3 position, int strawberries)
    {
        PlayerPrefs.SetFloat("PlayerX", position.x);
        PlayerPrefs.SetFloat("PlayerY", position.y);
        PlayerPrefs.SetFloat("PlayerZ", position.z);

        PlayerPrefs.SetInt("Strawberries", strawberries);

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

    // -------------------------
    // PICKUP SAVE / LOAD
    // -------------------------
    public static void MarkPickupCollected(string id)
    {
        PlayerPrefs.SetInt("pickup_" + id, 1);
    }

    public static bool IsPickupCollected(string id)
    {
        return PlayerPrefs.GetInt("pickup_" + id, 0) == 1;
    }

    // -------------------------
    // FULL GAME RESET
    // -------------------------
    public static void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
