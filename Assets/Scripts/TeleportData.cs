using UnityEngine;

public static class TeleportData
{
    public static bool useTeleportPosition = false;
    public static Vector3 spawnPosition = Vector3.zero;

    public static void SetTeleportPosition(Vector3 position)
    {
        spawnPosition = position;
        useTeleportPosition = true;
    }

    public static void Clear()
    {
        useTeleportPosition = false;
        spawnPosition = Vector3.zero;
    }
}
