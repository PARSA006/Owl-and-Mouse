using System.Collections.Generic;
using UnityEngine;

public class CheckpointSnapshot
{
    public Vector3 playerPosition;
    public int strawberryCount;

    public List<EnemySnapshot> enemies = new List<EnemySnapshot>();
    public List<TrapSnapshot> traps = new List<TrapSnapshot>();
    public List<string> collectedPickups = new List<string>();
}

public class EnemySnapshot
{
    public Vector3 position;
    public int patrolIndex;
    public EnemyState state;
}

public class TrapSnapshot
{
    public bool triggered;
}
