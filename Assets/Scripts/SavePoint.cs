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
            SaveSnapshot();
        }
    }

    private void SaveSnapshot()
    {
        Checkpoint.lastSnapshot = new CheckpointSnapshot();

        Checkpoint.lastSnapshot.playerPosition = PlayerMovement.Instance.transform.position;

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

            Checkpoint.lastSnapshot.enemies.Add(snap);
        }

        var allTraps = FindObjectsByType<Trap>(FindObjectsSortMode.None);

        foreach (var trap in allTraps)
        {
            TrapSnapshot ts = new TrapSnapshot
            {
                triggered = trap.triggered
            };

            Checkpoint.lastSnapshot.traps.Add(ts);
        }
    }
}
