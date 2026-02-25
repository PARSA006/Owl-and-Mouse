using System.Collections.Generic;
using UnityEngine;

// A container class that stores ALL data needed to restore the game state
// when the player respawns at a checkpoint.
// This is NOT a MonoBehaviour — it's just a data structure.
public class CheckpointSnapshot
{
    // The player's world position at the moment the checkpoint was saved.
    public Vector3 playerPosition;

    // How many strawberries the player had collected at the checkpoint.
    public int strawberryCount;

    // A list of saved enemy states.
    // Each entry corresponds to one enemy in the scene.
    public List<EnemySnapshot> enemies = new List<EnemySnapshot>();

    // A list of saved trap states.
    // Each entry stores whether a trap was triggered or not.
    public List<TrapSnapshot> traps = new List<TrapSnapshot>();

    // A list of pickup IDs that were collected at the checkpoint.
    // This ensures pickups do NOT respawn after restoring.
    public List<string> collectedPickups = new List<string>();
}

// A simple data class that stores the state of a trap at the checkpoint.
public class TrapSnapshot
{
    // Whether the trap was triggered (true) or still active (false).
    public bool triggered;
}
