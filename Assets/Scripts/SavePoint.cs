using UnityEngine;

public class SavePoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("SAVEPOINT TRIGGERED by: " + other.name);

        Transform root = other.transform.root;

        if (!root.CompareTag("Player"))
            return;

        PlayerInventory inv = root.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            SaveManager.SavePlayer(root.position, inv.strawberries);
            SaveSnapshot(inv);
        }
    }

    private void SaveSnapshot(PlayerInventory inv)
    {
        Debug.Log("CHECKPOINT SNAPSHOT CREATED");

        var snapshot = new CheckpointSnapshot();

        // PLAYER
        snapshot.playerPosition = PlayerMovement.Instance.transform.position;
        snapshot.strawberryCount = inv.strawberries;

        // ENEMIES
        var allEnemies = FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);
        foreach (var enemy in allEnemies)
        {
            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();

            EnemySnapshot snap = new EnemySnapshot
            {
                position = agent != null ? agent.nextPosition : enemy.transform.position,
                patrolIndex = enemy.GetActualTargetIndex(),
                state = EnemyState.Patrolling,
                zoneIndex = enemy.currentZoneIndex // NEW
            };

            snapshot.enemies.Add(snap);
        }

        // TRAPS
        var allTraps = FindObjectsByType<Trap>(FindObjectsSortMode.None);
        foreach (var trap in allTraps)
        {
            TrapSnapshot ts = new TrapSnapshot
            {
                triggered = trap.triggered
            };

            snapshot.traps.Add(ts);
        }

        // PICKUPS
        foreach (string id in StrawberryPickup.AllPickupIDs)
        {
            GameObject pickupObj = GameObject.Find(id);

            if (pickupObj == null)
            {
                snapshot.collectedPickups.Add(id);
                SaveManager.MarkPickupCollected(id);
            }
        }

        Checkpoint.SetSnapshot(snapshot);
    }
}
