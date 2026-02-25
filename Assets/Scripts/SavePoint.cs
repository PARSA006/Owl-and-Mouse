using UnityEngine;

// A SavePoint is a trigger that creates a checkpoint snapshot when the player touches it.
// It saves player position, inventory, enemy states, trap states, and collected pickups.
public class SavePoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("SAVEPOINT TRIGGERED by: " + other.name);

        // Some objects have child colliders, so we use root to ensure we detect the actual player object.
        Transform root = other.transform.root;

        // Only the player can activate a save point.
        if (!root.CompareTag("Player"))
            return;

        // Get the player's inventory so we can save strawberry count.
        PlayerInventory inv = root.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            // Save player position + strawberries to PlayerPrefs (persistent save).
            SaveManager.SavePlayer(root.position, inv.strawberries);

            // Create a full checkpoint snapshot (in-memory save).
            SaveSnapshot(inv);
        }
    }

    // Creates a full checkpoint snapshot containing:
    // - Player position
    // - Inventory
    // - Enemy states
    // - Trap states
    // - Collected pickups
    private void SaveSnapshot(PlayerInventory inv)
    {
        Debug.Log("CHECKPOINT SNAPSHOT CREATED");

        var snapshot = new CheckpointSnapshot();

        // -------------------------
        // PLAYER DATA
        // -------------------------

        // Save the player's current world position.
        snapshot.playerPosition = PlayerMovement.Instance.transform.position;

        // Save strawberry count.
        snapshot.strawberryCount = inv.strawberries;

        // -------------------------
        // ENEMY DATA
        // -------------------------

        // Find all enemies in the scene.
        var allEnemies = FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);

        foreach (var enemy in allEnemies)
        {
            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();

            // Create a snapshot for this enemy.
            EnemySnapshot snap = new EnemySnapshot
            {
                // Use agent.nextPosition if available (more accurate than transform.position).
                position = agent != null ? agent.nextPosition : enemy.transform.position,

                // Save which patrol point the enemy was heading toward.
                patrolIndex = enemy.GetActualTargetIndex(),

                // Always restore enemies to patrolling state (prevents weird mid-chase restores).
                state = EnemyState.Patrolling,

                // Save which patrol zone the enemy belongs to.
                zoneIndex = enemy.currentZoneIndex
            };

            snapshot.enemies.Add(snap);
        }

        // -------------------------
        // TRAP DATA
        // -------------------------

        var allTraps = FindObjectsByType<Trap>(FindObjectsSortMode.None);

        foreach (var trap in allTraps)
        {
            TrapSnapshot ts = new TrapSnapshot
            {
                // Save whether the trap was triggered.
                triggered = trap.triggered
            };

            snapshot.traps.Add(ts);
        }

        // -------------------------
        // PICKUP DATA
        // -------------------------

        // Loop through all pickup IDs in the game.
        foreach (string id in StrawberryPickup.AllPickupIDs)
        {
            // If the pickup object does NOT exist in the scene,
            // it means the player already collected it.
            GameObject pickupObj = GameObject.Find(id);

            if (pickupObj == null)
            {
                // Mark it as collected in the snapshot.
                snapshot.collectedPickups.Add(id);

                // Also mark it in SaveManager so it persists across scenes.
                SaveManager.MarkPickupCollected(id);
            }
        }

        // Store the snapshot in memory so PlayerRespawn can restore it later.
        Checkpoint.SetSnapshot(snapshot);
    }
}
