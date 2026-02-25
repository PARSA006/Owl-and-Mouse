using UnityEngine;

// A simple static class used to store a teleport position
// when transitioning between scenes. This allows the player
// to spawn at a specific location in the next scene.
public static class TeleportData
{
    // Whether a teleport position should be used when the next scene loads.
    public static bool useTeleportPosition = false;

    // The position where the player should spawn in the next scene.
    public static Vector3 spawnPosition = Vector3.zero;

    // Called before loading a new scene to set the desired spawn point.
    public static void SetTeleportPosition(Vector3 position)
    {
        spawnPosition = position;
        useTeleportPosition = true;
    }

    // Clears teleport data so future scene loads do not use a forced spawn point.
    public static void Clear()
    {
        useTeleportPosition = false;
        spawnPosition = Vector3.zero;
    }
}
