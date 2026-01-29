using UnityEngine;

public static class Checkpoint
{
    public static CheckpointSnapshot lastSnapshot;

    public static void SetSnapshot(CheckpointSnapshot snapshot)
    {
        lastSnapshot = snapshot;
    }

    public static void ClearSnapshot()
    {
        lastSnapshot = null;
    }
}
