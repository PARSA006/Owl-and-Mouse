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

    // All enemies in the scene at the moment of saving
    public List<EnemySnapshot> enemies = new List<EnemySnapshot>();

    // All traps in the scene at the moment of saving
    public List<TrapSnapshot> traps = new List<TrapSnapshot>();
}
