using UnityEngine;

public static class TeleportData
{
    // Whether the next scene load should use the stored spawn position
    public static bool useTeleportPosition = false;

    // The position the player should spawn at
    public static Vector3 spawnPosition = Vector3.zero;

    // Helper method to set teleport data safely
    public static void SetTeleportPosition(Vector3 position)
    {
        spawnPosition = position;
        useTeleportPosition = true;
    }

    // Helper method to clear teleport data after use
    public static void Clear()
    {
        useTeleportPosition = false;
        spawnPosition = Vector3.zero;
    }
}
