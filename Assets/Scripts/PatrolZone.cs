using UnityEngine;

public class PatrolZone : MonoBehaviour
{
    public int zoneIndex;
    public Transform[] patrolPoints;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // NEW: Modern, non-obsolete API
        var enemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            enemy.SwitchToZone(zoneIndex, patrolPoints);
        }
    }
}
