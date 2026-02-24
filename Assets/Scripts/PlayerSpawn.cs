using UnityEngine;
using System.Collections;

public class SceneStartSpawn : MonoBehaviour
{
    [SerializeField] private Transform startPosition;

    private void Start()
    {
        StartCoroutine(SpawnWhenReady());
    }

    private IEnumerator SpawnWhenReady()
    {
        // Wait 1 frame so CharacterController initializes
        yield return null;

        var player = FindFirstObjectByType<PlayerMovement>();
        if (player == null)
            yield break;

        var controller = player.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        // Apply spawn position
        player.transform.position = startPosition.position;

        // Re-enable controller
        if (controller != null)
            controller.enabled = true;
    }
}
