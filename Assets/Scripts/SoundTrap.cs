using UnityEngine;

// A trap that makes a noise when the player enters it.
// Nearby enemies will hear the sound and move to investigate.
public class SoundTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private AudioSource soundEffect; // Sound played when triggered
    [SerializeField] private float alertRadius = 20f; // How far enemies can hear the sound

    private void OnTriggerEnter(Collider other)
    {
        // Only the player can trigger the sound trap
        if (!other.CompareTag("Player"))
            return;

        // Play the trap sound if assigned
        if (soundEffect != null)
            soundEffect.Play();

        // Notify all enemies within range
        AlertEnemies();
    }

    // Finds all enemies and alerts only those within hearing distance
    private void AlertEnemies()
    {
        // Get all enemies in the scene
        var enemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            // Distance from trap to enemy
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            // If enemy is close enough, send them to investigate the sound
            if (dist <= alertRadius)
                enemy.InvestigateSound(transform.position);
        }
    }
}
