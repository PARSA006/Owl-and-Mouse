using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hazard"))
        {
            PlayerRespawn.Instance.RespawnPlayer();
        }
    }
}
