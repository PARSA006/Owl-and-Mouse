using UnityEngine;
using UnityEngine.Video;

// Controls a cutscene that plays using a VideoPlayer.
// Disables player movement during the cutscene and re-enables it when the video ends.
public class CutsceneController : MonoBehaviour
{
    // Reference to the VideoPlayer component that plays the cutscene.
    [SerializeField] private VideoPlayer videoPlayer;

    // Reference to the player's movement script so we can disable/enable control.
    [SerializeField] private PlayerMovement playerMovement;

    private void Start()
    {
        // When the cutscene starts, disable player movement so the player can't move.
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Subscribe to the VideoPlayer event that fires when the video finishes.
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    // This method is automatically called by Unity when the video reaches its end.
    private void OnVideoFinished(VideoPlayer vp)
    {
        EndCutscene();
    }

    // Ends the cutscene and returns control to the player.
    private void EndCutscene()
    {
        // Re-enable player movement so the player can move again.
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Disable this GameObject (usually the cutscene canvas or UI).
        // This hides the cutscene from the screen.
        gameObject.SetActive(false);
    }
}
