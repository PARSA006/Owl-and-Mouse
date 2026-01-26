using UnityEngine;

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
    public NewMonoBehaviourScript enemyAI;

    private Mesh mesh;
    private bool playerVisible;
    private bool wasPlayerVisible = false;   // ⭐ NEW

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "SkyLightConeMesh";
        GetComponent<MeshFilter>().mesh = mesh;
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
        // ⭐ Only fire events when visibility CHANGES
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
