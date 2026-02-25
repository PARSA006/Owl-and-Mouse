using UnityEngine;

[System.Serializable]
public class EnemySnapshot
{
    // The enemy's world position at the moment the checkpoint was saved.
    public Vector3 position;

    // Which patrol point the enemy was heading toward.
    // This helps restore patrol progress instead of restarting the route.
    public int patrolIndex;

    // The AI state at the moment of saving (Patrolling, Following, Investigating, etc.)
    // This allows the enemy to resume the correct behavior after respawn.
    public EnemyState state;

    // The patrol zone the enemy belonged to.
    // Needed because enemies can switch zones dynamically.
    public int zoneIndex;
}
