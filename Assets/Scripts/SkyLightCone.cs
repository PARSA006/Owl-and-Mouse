using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkyLightCone : MonoBehaviour
{
    [Header("Cone Shape")]
    [SerializeField] private float height = 10f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private int segments = 40;

    [Header("Detection")]
    [SerializeField] private LayerMask environmentMask;
    [SerializeField] private LayerMask playerMask;

    private NewMonoBehaviourScript enemyAI;
    private Mesh mesh;

    private bool playerVisible = false;
    private bool wasPlayerVisible = false;

    private IEnumerator Start()
    {
        mesh = new Mesh { name = "SkyLightConeMesh" };
        GetComponent<MeshFilter>().mesh = mesh;

        // Wait 1 frame so enemies can spawn after scene load
        yield return null;

        enemyAI = FindFirstObjectByType<NewMonoBehaviourScript>();
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

                if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
                    playerVisible = true;
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
            enemyAI = FindFirstObjectByType<NewMonoBehaviourScript>();
            if (enemyAI == null)
                return;
        }

        if (playerVisible && !wasPlayerVisible)
        {
            enemyAI.PlayerEnteredCone();
        }
        else if (!playerVisible && wasPlayerVisible)
        {
            enemyAI.PlayerExitedCone();
        }

        wasPlayerVisible = playerVisible;
    }
}
