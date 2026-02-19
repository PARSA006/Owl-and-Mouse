using UnityEngine;

public class TeleportReceiver : MonoBehaviour
{
    private void Start()
    {
        if (TeleportData.useTeleportPosition)
        {
            var player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                var controller = player.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;

                player.transform.position = TeleportData.spawnPosition;

                if (controller != null) controller.enabled = true;
            }

            TeleportData.Clear();
        }
    }
}
