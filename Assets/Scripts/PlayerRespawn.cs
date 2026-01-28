using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerRespawn
{
    public static void RespawnPlayer()
    {
        // If you have a saved checkpoint, load it
        if (SaveManager.HasSave())
        {
            Vector3 savedPos = SaveManager.LoadPlayerPosition();

            // Move the player there
            var player = Object.FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                var controller = player.GetComponent<CharacterController>();
                controller.enabled = false;
                player.transform.position = savedPos;
                controller.enabled = true;
            }
        }
        else
        {
            // No checkpoint → restart the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
