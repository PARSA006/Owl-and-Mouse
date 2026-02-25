using System.Collections;
using UnityEngine;

// This script generates a dynamic cone mesh that acts like a spotlight or detection cone.
// It raycasts around the cone to see if the player is inside the light.
// If the player enters or exits the cone, it notifies the enemy AI.

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkyLightCone : MonoBehaviour
{
    [Header("Cone Shape")]
    [SerializeField] private float height = 10f;      // How tall the cone is
    [SerializeField] private float radius = 5f;       // Radius of the cone base
    [SerializeField] private int segments = 40;       // Number of segments around the circle (smoothness)

    [Header("Detection")]
    [SerializeField] private LayerMask environmentMask; // Layers that block the cone (walls, obstacles)
    [SerializeField] private LayerMask playerMask;      // Layer for detecting the player

    private NewMonoBehaviourScript enemyAI; // Reference to the enemy AI script
    private Mesh mesh;                      // Mesh used to draw the cone

    private bool playerVisible = false;     // Is the player currently inside the cone?
    private bool wasPlayerVisible = false;  // Was the player inside the cone last frame?

    private IEnumerator Start()
    {
        // Create a new mesh for the cone
        mesh = new Mesh { name = "SkyLightConeMesh" };
        GetComponent<MeshFilter>().mesh = mesh;

        // Wait 1 frame so enemies have time to spawn after scene load
        yield return null;

        // Find the enemy AI in the scene
        enemyAI = FindFirstObjectByType<NewMonoBehaviourScript>();
    }

    private void LateUpdate()
    {
        // Generate the cone mesh every frame
        // (LateUpdate ensures it updates after movement)
        GenerateCone();
    }

    private void GenerateCone()
    {
        Vector3 origin = transform.position;

        // Apply object scaling to height and radius
        float scaledHeight = height * transform.localScale.y;
        float scaledRadius = radius * Mathf.Max(transform.localScale.x, transform.localScale.z);

        // Vertex count: 1 center vertex + 1 vertex per segment
        int vertexCount = segments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segments * 3];

        // First vertex is the cone tip (local space)
        vertices[0] = Vector3.zero;

        playerVisible = false;

        // Loop around the circle to generate the cone base
        for (int i = 0; i <= segments; i++)
        {
            // Angle around the circle
            float angle = (i / (float)segments) * Mathf.PI * 2f;

            // Local position of the circle point
            Vector3 localCirclePos = new Vector3(
                Mathf.Cos(angle) * scaledRadius,
                -scaledHeight,
                Mathf.Sin(angle) * scaledRadius
            );

            // Convert to world space
            Vector3 worldCirclePos = transform.TransformPoint(localCirclePos);

            // Direction from cone tip to circle point
            Vector3 worldDir = (worldCirclePos - origin).normalized;

            // Default hit point is the full cone length
            Vector3 hitPoint = origin + worldDir * scaledHeight;

            // Raycast to detect walls or player
            if (Physics.Raycast(origin, worldDir, out RaycastHit hit, scaledHeight, environmentMask | playerMask))
            {
                hitPoint = hit.point;

                // Check if the hit object is the player
                if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
                    playerVisible = true;
            }

            // Convert hit point back to local space for the mesh
            vertices[i + 1] = transform.InverseTransformPoint(hitPoint);
        }

        // Build triangle indices for the cone mesh
        for (int i = 0; i < segments; i++)
        {
            int v = i + 1;
            triangles[i * 3] = 0;       // Cone tip
            triangles[i * 3 + 1] = v;   // Current segment
            triangles[i * 3 + 2] = v + 1; // Next segment
        }

        // Apply mesh data
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Handle detection events
        HandleDetection();
    }

    private void HandleDetection()
    {
        // If AI reference is missing, try to find it again
        if (enemyAI == null)
        {
            enemyAI = FindFirstObjectByType<NewMonoBehaviourScript>();
            if (enemyAI == null)
                return;
        }

        // Player just entered the cone
        if (playerVisible && !wasPlayerVisible)
        {
            enemyAI.PlayerEnteredCone();
        }
        // Player just exited the cone
        else if (!playerVisible && wasPlayerVisible)
        {
            enemyAI.PlayerExitedCone();
        }

        // Store state for next frame
        wasPlayerVisible = playerVisible;
    }
}
