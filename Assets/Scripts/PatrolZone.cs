using UnityEngine;

public class PatrolZone : MonoBehaviour
{
    public int zoneIndex;
    public Transform[] patrolPoints;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Zone " + zoneIndex + " triggered by: " + other.name + " (tag: " + other.tag + ")");

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Ignored because it is not the Player.");
            return;
        }

        Debug.Log("Player entered zone " + zoneIndex + ". Switching enemy to this zone.");

        var enemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);

        Debug.Log("Found " + enemies.Length + " enemies in scene.");

        foreach (var enemy in enemies)
        {
            Debug.Log("Sending patrol points to enemy: " + enemy.name);
            enemy.SwitchToZone(zoneIndex, patrolPoints);
        }
    }
}
