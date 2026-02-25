using UnityEngine;

// This script detects when the player touches something that should kill them.
// It expects the player to have a trigger collider (or enter one).
public class PlayerDeath : MonoBehaviour
{
    // Called automatically by Unity when this object enters a trigger collider.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we collided with has the tag "Hazard".
        // This prevents random objects from killing the player.
        if (other.CompareTag("Hazard"))
        {
            // Call the respawn system to reset the player to the last checkpoint.
            // PlayerRespawn is assumed to be a singleton with a RespawnPlayer() method.
            PlayerRespawn.Instance.RespawnPlayer();
        }
    }
}
