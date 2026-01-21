using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkyLightCone : MonoBehaviour
{
    [Header("Cone Shape")]
    public float viewRadius = 12f;       // How far the cone reaches
    public float viewAngle = 60f;        // Width of the cone (degrees)
    public int rayCount = 60;            // Smoothness of the cone mesh

    [Header("Cone Orientation")]
    public float tiltAngle = -30f;       // Tilt downward (negative = down)
    public float rotationOffset = 0f;    // Rotate cone left/right

    [Header("Detection")]
    public LayerMask environmentMask;    // Walls, ground, props
    public LayerMask playerMask;         // Player layer
    public NewMonoBehaviourScript enemyAI;

    private Mesh mesh;
    private bool playerVisible;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "DynamicLightConeMesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void LateUpdate()
    {
        GenerateCone();
    }

    private void GenerateCone()
    {
        Vector3 origin = transform.position;

        int vertexCount = rayCount + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // center of cone (local space)

        float halfAngle = viewAngle * 0.5f;
        playerVisible = false;

        for (int i = 0; i <= rayCount; i++)
        {
            float t = i / (float)rayCount;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);

            // Build direction in LOCAL SPACE
            Vector3 dirLocal =
                Quaternion.Euler(tiltAngle, angle + rotationOffset, 0f)
                * Vector3.forward;

            // Convert to WORLD SPACE for raycast
            Vector3 worldDir = transform.TransformDirection(dirLocal);

            Vector3 hitPoint = origin + worldDir * viewRadius;

            // Raycast for environment or player
            if (Physics.Raycast(origin, worldDir, out RaycastHit hit, viewRadius, environmentMask | playerMask))
            {
                hitPoint = hit.point;

                // If we hit the player first
                if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
                {
                    playerVisible = true;
                }
            }

            // Convert hit point back to LOCAL SPACE for mesh
            vertices[i + 1] = transform.InverseTransformPoint(hitPoint);
        }

        // Build triangles
        for (int i = 0; i < rayCount; i++)
        {
            int vert = i + 1;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = vert;
            triangles[i * 3 + 2] = vert + 1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        HandleDetection();
    }

    private void HandleDetection()
    {
        if (playerVisible)
            enemyAI.PlayerEnteredCone();
        else
            enemyAI.PlayerExitedCone();
    }
}
