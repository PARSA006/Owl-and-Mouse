using UnityEngine;

public class VisionConeTrigger : MonoBehaviour
{
    public NewMonoBehaviourScript enemyAI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyAI.PlayerEnteredCone();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyAI.PlayerExitedCone();
        }
    }
}
