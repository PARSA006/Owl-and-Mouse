using UnityEngine;

// A patrol zone defines a region in the world that the player can enter.
// When the player enters this zone, all enemies switch to the patrol points
// assigned to this zone. This allows dynamic enemy routing based on player movement.
public class PatrolZone : MonoBehaviour
{
    // Unique identifier for this zone.
    // Enemies use this to know which zone they belong to.
    public int zoneIndex;

    // The patrol points that enemies should follow when they switch to this zone.
    public Transform[] patrolPoints;

    private void OnTriggerEnter(Collider other)
    {
        // Debug info to help track which object entered the zone.
        Debug.Log("Zone " + zoneIndex + " triggered by: " + other.name + " (tag: " + other.tag + ")");

        // Only react when the PLAYER enters the zone.
        if (!other.CompareTag("Player"))
        {
            Debug.Log("Ignored because it is not the Player.");
            return;
        }

        Debug.Log("Player entered zone " + zoneIndex + ". Switching enemy to this zone.");

        // Find all enemies currently in the scene.
        var enemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);

        Debug.Log("Found " + enemies.Length + " enemies in scene.");

        // Tell every enemy to switch to this zone's patrol points.
        foreach (var enemy in enemies)
        {
            Debug.Log("Sending patrol points to enemy: " + enemy.name);
            enemy.SwitchToZone(zoneIndex, patrolPoints);
        }
    }
}
