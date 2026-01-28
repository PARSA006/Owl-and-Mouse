using UnityEngine;

public class Trap : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tell the respawn system to reload the last checkpoint
            PlayerRespawn.RespawnPlayer();
        }
    }
}
