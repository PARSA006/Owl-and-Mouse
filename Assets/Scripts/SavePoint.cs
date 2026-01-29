using UnityEngine;

public class SavePoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            SaveManager.SavePlayer(other.transform.position, inv.strawberries);
            SaveSnapshot(inv);
        }
    }

    private void SaveSnapshot(PlayerInventory inv)
    {
        var snapshot = new CheckpointSnapshot();
        snapshot.playerPosition = PlayerMovement.Instance.transform.position;

        // -------------------------
        // SAVE PLAYER INVENTORY
        // -------------------------
        snapshot.strawberryCount = inv.strawberries;

        // -------------------------
        // ENEMIES
        // -------------------------
        var allEnemies = FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);
        foreach (var enemy in allEnemies)
        {
            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();

            EnemySnapshot snap = new EnemySnapshot
            {
                position = agent != null ? agent.nextPosition : enemy.transform.position,
                patrolIndex = enemy.GetActualTargetIndex(),
                state = EnemyState.Patrolling
            };

            snapshot.enemies.Add(snap);
        }

        // -------------------------
        // TRAPS
        // -------------------------
        var allTraps = FindObjectsByType<Trap>(FindObjectsSortMode.None);
        foreach (var trap in allTraps)
        {
            TrapSnapshot ts = new TrapSnapshot
            {
                triggered = trap.triggered
            };

            snapshot.traps.Add(ts);
        }

        // -------------------------
        // PICKUPS
        // -------------------------
        var pickups = FindObjectsByType<StrawberryPickup>(FindObjectsSortMode.None);
        foreach (var pickup in pickups)
        {
            if (SaveManager.IsPickupCollected(pickup.PickupID))
                snapshot.collectedPickups.Add(pickup.PickupID);
        }

        Checkpoint.SetSnapshot(snapshot);
    }
}
