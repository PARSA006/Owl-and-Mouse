using UnityEngine;

public class SoundTrap : MonoBehaviour
{
    public AudioSource soundEffect;
    public float alertRadius = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Play the trap sound
            if (soundEffect != null)
                soundEffect.Play();

            // Alert all enemies in range
            AlertEnemies();
        }
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
