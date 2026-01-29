using UnityEngine;

public class SoundTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private AudioSource soundEffect;
    [SerializeField] private float alertRadius = 20f;

    private bool hasTriggered = false; // prevents double-triggering

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return; // avoid multiple triggers
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;

        // Play the trap sound
        if (soundEffect != null)
            soundEffect.Play();

        // Alert all enemies in range
        AlertEnemies();
    }

    private void AlertEnemies()
    {
        var enemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            if (dist <= alertRadius)
            {
                enemy.InvestigateSound(transform.position);
            }
        }
    }
}
