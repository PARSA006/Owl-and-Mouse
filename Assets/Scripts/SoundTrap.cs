using UnityEngine;

public class SoundTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private AudioSource soundEffect;
    [SerializeField] private float alertRadius = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (soundEffect != null)
            soundEffect.Play();

        AlertEnemies();
    }

    private void AlertEnemies()
    {
        var enemies = Object.FindObjectsByType<NewMonoBehaviourScript>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= alertRadius)
                enemy.InvestigateSound(transform.position);
        }
    }
}
