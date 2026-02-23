using UnityEngine;
using UnityEngine.Video;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private PlayerMovement playerMovement; // assign your player here

    private void Start()
    {
        // Disable player movement during cutscene
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Listen for video end
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        EndCutscene();
    }

    private void EndCutscene()
    {
        // Enable player movement
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Hide cutscene canvas
        gameObject.SetActive(false);
    }
}
