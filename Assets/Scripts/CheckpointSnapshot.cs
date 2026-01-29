using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySnapshot
{
    public Vector3 position;
    public int patrolIndex;
    public EnemyState state;
}

[System.Serializable]
public class TrapSnapshot
{
    public bool triggered;
}

[System.Serializable]
public class CheckpointSnapshot
{
    public Vector3 playerPosition;

    public List<EnemySnapshot> enemies = new List<EnemySnapshot>();
    public List<TrapSnapshot> traps = new List<TrapSnapshot>();

    // Track which pickups were collected at the moment of saving
    public HashSet<string> collectedPickups = new HashSet<string>();

    // NEW: Track how many strawberries the player had at the checkpoint
    public int strawberryCount;
}
