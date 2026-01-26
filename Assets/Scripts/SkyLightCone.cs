using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkyLightCone : MonoBehaviour
{
    [Header("Cone Shape")]
    public float height = 10f;
    public float radius = 5f;
    public int segments = 40;

    [Header("Detection")]
    public LayerMask environmentMask;
    public LayerMask playerMask;

    private NewMonoBehaviourScript enemyAI;

    private Mesh mesh;
    private bool playerVisible;
    private bool wasPlayerVisible = false;

    private IEnumerator Start()
    {
        mesh = new Mesh();
        mesh.name = "SkyLightConeMesh";
        GetComponent<MeshFilter>().mesh = mesh;

        // Wait 1 frame so the enemy has time to spawn in the new scene
        yield return null;

        enemyAI = FindFirstObjectByType<NewMonoBehaviourScript>();

        Debug.Log("SkyLightCone Start() — enemyAI found: " + (enemyAI != null));
    }

    private void LateUpdate()
    {
        GenerateCone();
    }

    private void GenerateCone()
    {
        Vector3 origin = transform.position;

        float scaledHeight = height * transform.localScale.y;
        float scaledRadius = radius * Mathf.Max(transform.localScale.x, transform.localScale.z);

        int vertexCount = segments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        playerVisible = false;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;

            Vector3 localCirclePos = new Vector3(
                Mathf.Cos(angle) * scaledRadius,
                -scaledHeight,
                Mathf.Sin(angle) * scaledRadius
            );

            Vector3 worldCirclePos = transform.TransformPoint(localCirclePos);
            Vector3 worldDir = (worldCirclePos - origin).normalized;

            Vector3 hitPoint = origin + worldDir * scaledHeight;

            if (Physics.Raycast(origin, worldDir, out RaycastHit hit, scaledHeight, environmentMask | playerMask))
            {
                hitPoint = hit.point;

                // DEBUG: What did the cone hit?
                Debug.Log("Cone hit: " + hit.collider.name + " (Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer) + ")");

                if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
                {
                    playerVisible = true;
                    Debug.Log("Cone sees PLAYER in scene: " + gameObject.scene.name);
                }
            }

            vertices[i + 1] = transform.InverseTransformPoint(hitPoint);
        }

        for (int i = 0; i < segments; i++)
        {
            int v = i + 1;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = v;
            triangles[i * 3 + 2] = v + 1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        HandleDetection();
    }

    private void HandleDetection()
    {
        if (enemyAI == null)
        {
            Debug.LogWarning("SkyLightCone has NO enemyAI in scene: " + gameObject.scene.name);
            return;
        }

        if (playerVisible && !wasPlayerVisible)
        {
            Debug.Log("PlayerEnteredCone fired");
            enemyAI.PlayerEnteredCone();
        }
        else if (!playerVisible && wasPlayerVisible)
        {
            Debug.Log("PlayerExitedCone fired");
            enemyAI.PlayerExitedCone();
        }

        wasPlayerVisible = playerVisible;
    }
}
