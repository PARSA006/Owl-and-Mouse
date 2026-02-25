using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// The different states the enemy can be in.
public enum EnemyState
{
    Patrolling,
    Following,
    Attacking,
    Investigating
}

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Timer used while the enemy is in "Investigating" state.
    private float investigateTimer = 0f;

    // Cached animator parameter ID for performance.
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    // -------------------------
    // INSPECTOR REFERENCES
    // -------------------------

    [Header("References")]
    [SerializeField] private Transform player; // The player the enemy will chase.

    [Header("Default Patrol Points (Zone 0)")]
    [SerializeField] private Transform[] patrolPoints; // Default patrol route.

    [Header("Audio")]
    [SerializeField] private AudioSource chaseMusic;       // Music played during chase.
    [SerializeField] private AudioSource investigateSound; // Sound played when investigating.

    [Header("Vision Cone Fade")]
    [SerializeField] private Renderer[] coneRenderers; // Renderers for the vision cone.
    [SerializeField] private float coneFadeSpeed = 3f; // How fast the cone fades.

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 2f;       // Speed while patrolling.
    [SerializeField] private float chaseStartSpeed = 4f;   // Initial chase speed.
    [SerializeField] private float maxChaseSpeed = 8f;     // Max chase speed.

    [Header("Investigation Settings")]
    [SerializeField] private float investigateSpeed = 3f;  // Speed while investigating.

    [Header("Chase Acceleration")]
    [SerializeField] private float accelerationTime = 1.5f;     // Time to accelerate.
    [SerializeField] private float chaseAccelerationRate = 0.5f; // How fast speed increases.

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 720f; // Rotation speed.

    [Header("Settings")]
    [SerializeField] private float patrolWaitTime = 2f; // Wait time at patrol points.
    [SerializeField] private float stopAtDistance = 0.5f; // Distance to stop from target.
    [SerializeField] private float losePlayerTime = 3f;   // Time before giving up chase.
    [SerializeField] private float attackRange = 1.2f;    // Distance required to attack.

    // -------------------------
    // INTERNAL COMPONENTS
    // -------------------------

    private NavMeshAgent agent;   // Handles pathfinding.
    private Animator animator;    // Controls animations.
    private EnemyState state = EnemyState.Patrolling; // Current AI state.

    private int patrolIndex; // Current patrol point index.
    private bool isWaiting;  // Whether the enemy is waiting at a patrol point.

    // Coroutines used for fading, acceleration, and losing the player.
    private Coroutine fadeRoutine;
    private Coroutine accelRoutine;
    private Coroutine loseRoutine;

    private bool chaseStarted = false;     // Whether chase music has started.
    private bool playerInCone = false;     // Whether player is inside vision cone.
    private bool hasPlayedInvestigateSound = false; // Prevents repeating sound.

    // -------------------------
    // ZONE SYSTEM
    // -------------------------

    public int currentZoneIndex = 0;           // Which zone the enemy belongs to.
    public Transform[] currentPatrolPoints;    // Patrol points for the current zone.

    // Called when the enemy switches to a new patrol zone.
    public void SwitchToZone(int zoneIndex, Transform[] newPoints)
    {
        Debug.Log("Zone " + zoneIndex + " has " + currentPatrolPoints.Length + " patrol points.");

        currentZoneIndex = zoneIndex;
        currentPatrolPoints = newPoints;

        // If the zone has no patrol points, do nothing.
        if (currentPatrolPoints == null || currentPatrolPoints.Length == 0)
            return;

        // Find the closest patrol point to join the new zone smoothly.
        int closest = 0;
        float bestDist = float.MaxValue;

        for (int i = 0; i < currentPatrolPoints.Length; i++)
        {
            float d = Vector3.Distance(transform.position, currentPatrolPoints[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                closest = i;
            }
        }

        patrolIndex = closest;

        // Reset movement and set new destination.
        agent.isStopped = false;
        agent.ResetPath();
        StartCoroutine(SetDestinationNextFrame(currentPatrolPoints[patrolIndex].position));

        Debug.Log("Enemy switched to zone " + zoneIndex);
    }

    // Waits one frame before setting the destination.
    // This avoids NavMeshAgent errors when switching zones.
    private IEnumerator SetDestinationNextFrame(Vector3 pos)
    {
        yield return null;
        agent.SetDestination(pos);
    }

    // Returns the correct patrol index depending on whether the enemy is waiting.
    public int GetActualTargetIndex()
    {
        if (isWaiting)
            return (patrolIndex + currentPatrolPoints.Length - 1) % currentPatrolPoints.Length;

        return patrolIndex;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Re-find player when a new scene loads.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Prevent memory leaks.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Delay one frame so player exists before searching.
        StartCoroutine(DelayedPlayerFind());
    }

    private IEnumerator DelayedPlayerFind()
    {
        yield return null;
        TryFindPlayer();
    }

    private void Start()
    {
        // Disable NavMeshAgent's automatic movement — we manually move the enemy.
        agent.updatePosition = false;
        agent.updateRotation = false;

        TryFindPlayer();

        // Configure movement settings.
        agent.speed = patrolSpeed;
        agent.angularSpeed = turnSpeed;
        agent.updateRotation = false;
        agent.autoBraking = false;
        agent.acceleration = 999f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.stoppingDistance = stopAtDistance;

        // Default zone is zone 0.
        currentPatrolPoints = patrolPoints;

        // If not restoring from checkpoint, start at patrol point 0.
        if (!PlayerRespawn.restoredFromCheckpoint)
        {
            patrolIndex = 0;
            GoToNextPatrolPoint();
        }

        // Fade cone to default color.
        FadeConeToColor(new Color(1f, 1f, 0f, 0.25f));
    }

    private Vector3 lastHorizontalDir = Vector3.zero;

    private void Update()
    {
        // ---------------------------
        // CUSTOM FLYING MOVEMENT
        // ---------------------------

        if (agent.hasPath)
        {
            Vector3 target = agent.steeringTarget;

            // Horizontal direction only (ignore Y).
            Vector3 horizontalDir = new Vector3(
                target.x - transform.position.x,
                0f,
                target.z - transform.position.z
            ).normalized;

            lastHorizontalDir = horizontalDir;

            // Move horizontally like a flying creature.
            transform.position += horizontalDir * agent.speed * Time.deltaTime;

            // ---------------------------
            // VERTICAL MOVEMENT
            // ---------------------------

            float desiredHeight;

            if (state == EnemyState.Following)
            {
                // While chasing, match player's height.
                desiredHeight = player.position.y + 1.5f;
            }
            else
            {
                // While patrolling, match patrol point height.
                desiredHeight = currentPatrolPoints[patrolIndex].position.y;
            }

            float verticalSpeed = 3f;

            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, desiredHeight, Time.deltaTime * verticalSpeed);
            transform.position = pos;

            // ---------------------------
            // ROTATION
            // ---------------------------

            if (horizontalDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizontalDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 3f);
            }
        }

        // ---------------------------
        // AI LOGIC
        // ---------------------------

        if (player == null)
        {
            TryFindPlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;

            case EnemyState.Following:
                FollowPlayer();

                // If close enough AND player is in cone → attack.
                if (dist <= attackRange && playerInCone)
                {
                    state = EnemyState.Attacking;
                    PlayerRespawn.Instance.RespawnPlayer();
                }
                break;

            case EnemyState.Investigating:
                Investigate();
                break;
        }

        UpdateAnimations();
    }

    // Attempts to find the player in the scene.
    // Called when the scene loads or if the player reference becomes null.
    private void TryFindPlayer()
    {
        var playerObj = FindFirstObjectByType<PlayerMovement>();
        if (playerObj != null)
            player = playerObj.transform;
    }

    // Smoothly rotates the enemy to face the player.
    private void RotateTowardPlayer()
    {
        if (player == null) return;

        // Direction to player (ignore vertical difference)
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;

        // Only rotate if direction is meaningful
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                turnSpeed * Time.deltaTime
            );
        }
    }

    // Called by the vision cone when the player enters the cone.
    public void PlayerEnteredCone()
    {
        playerInCone = true;

        // If already attacking, ignore
        if (state == EnemyState.Attacking) return;

        // Switch to chase mode
        state = EnemyState.Following;

        // Start chase acceleration if not already started
        if (!chaseStarted)
        {
            chaseStarted = true;

            if (accelRoutine != null)
                StopCoroutine(accelRoutine);

            accelRoutine = StartCoroutine(AccelerateToChaseStartSpeed());
        }

        // Cancel losing-player countdown
        if (loseRoutine != null)
        {
            StopCoroutine(loseRoutine);
            loseRoutine = null;
        }

        // Start chase music
        if (!chaseMusic.isPlaying)
            chaseMusic.Play();

        // Fade cone to red
        FadeConeToColor(new Color(1f, 0f, 0f, 0.35f));
    }

    // Called when the player leaves the vision cone.
    public void PlayerExitedCone()
    {
        playerInCone = false;

        // Only matters if currently chasing
        if (state != EnemyState.Following) return;

        // Restart lose-player countdown
        if (loseRoutine != null)
            StopCoroutine(loseRoutine);

        loseRoutine = StartCoroutine(LosePlayerRoutine());
    }

    // Waits a few seconds before giving up the chase.
    private IEnumerator LosePlayerRoutine()
    {
        float timer = 0f;

        while (timer < losePlayerTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // If still chasing after timer → return to patrol
        if (state == EnemyState.Following)
        {
            state = EnemyState.Patrolling;
            chaseStarted = false;

            if (chaseMusic.isPlaying)
                chaseMusic.Stop();

            FadeConeToColor(new Color(1f, 1f, 0f, 0.25f));

            agent.speed = patrolSpeed;
            agent.isStopped = false;

            GoToClosestPatrolPoint();
        }

        loseRoutine = null;
    }

    // Smoothly accelerates enemy from patrol speed → chase start speed.
    private IEnumerator AccelerateToChaseStartSpeed()
    {
        float start = patrolSpeed;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / accelerationTime;
            agent.speed = Mathf.Lerp(start, chaseStartSpeed, t);
            yield return null;
        }

        agent.speed = chaseStartSpeed;
    }

    // Main chase logic: follow the player's position.
    private void FollowPlayer()
    {
        if (player == null) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);

        // Gradually increase speed up to max chase speed.
        if (agent.speed < maxChaseSpeed)
            agent.speed += chaseAccelerationRate * Time.deltaTime;
    }

    // Patrol behavior: move between patrol points.
    private void Patrol()
    {
        investigateTimer = 0f;

        agent.isStopped = false;

        // Safety check: if path is broken, rebuild it
        if (agent.hasPath && float.IsInfinity(agent.remainingDistance))
        {
            agent.ResetPath();
            agent.SetDestination(currentPatrolPoints[patrolIndex].position);
            return;
        }

        if (isWaiting)
            return;

        // Arrived at patrol point
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    // Enemy waits at a patrol point before moving to the next.
    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(patrolWaitTime);

        // Move to next patrol point
        patrolIndex = (patrolIndex + 1) % currentPatrolPoints.Length;

        agent.isStopped = false;
        agent.ResetPath();
        yield return null;

        agent.SetDestination(currentPatrolPoints[patrolIndex].position);

        isWaiting = false;
    }

    // Finds the closest patrol point and moves there.
    private void GoToClosestPatrolPoint()
    {
        if (currentPatrolPoints == null || currentPatrolPoints.Length == 0) return;

        int closest = 0;
        float bestDist = float.MaxValue;

        for (int i = 0; i < currentPatrolPoints.Length; i++)
        {
            float d = Vector3.Distance(transform.position, currentPatrolPoints[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                closest = i;
            }
        }

        patrolIndex = closest;
        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(currentPatrolPoints[patrolIndex].position);
    }

    // Moves to the current patrol point (used on startup).
    private void GoToNextPatrolPoint()
    {
        if (currentPatrolPoints == null || currentPatrolPoints.Length == 0) return;
        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(currentPatrolPoints[patrolIndex].position);
    }

    // Updates walking animation based on movement state.
    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool walking =
            state == EnemyState.Following ||
            state == EnemyState.Investigating ||
            lastHorizontalDir.sqrMagnitude > 0.01f;

        animator.SetBool(IsWalking, walking);
    }

    // Fades the vision cone to a target color.
    private void FadeConeToColor(Color target)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeConeRoutine(target));
    }

    // Smooth color fade for the vision cone.
    private IEnumerator FadeConeRoutine(Color target)
    {
        Color[] start = new Color[coneRenderers.Length];

        // Store starting colors
        for (int i = 0; i < coneRenderers.Length; i++)
            start[i] = coneRenderers[i].material.GetColor("_BaseColor");

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * coneFadeSpeed;

            for (int i = 0; i < coneRenderers.Length; i++)
                coneRenderers[i].material.SetColor("_BaseColor", Color.Lerp(start[i], target, t));

            yield return null;
        }
    }

    // Called when a sound event triggers investigation.
    public void InvestigateSound(Vector3 soundPosition)
    {
        state = EnemyState.Investigating;
        playerInCone = false;

        agent.speed = investigateSpeed;

        // Cancel losing-player routine
        if (loseRoutine != null)
        {
            StopCoroutine(loseRoutine);
            loseRoutine = null;
        }

        agent.isStopped = false;
        agent.SetDestination(soundPosition);

        // Play investigate sound once
        if (!hasPlayedInvestigateSound && investigateSound != null)
        {
            investigateSound.Play();
            hasPlayedInvestigateSound = true;
        }

        // Fade cone to orange
        FadeConeToColor(new Color(1f, 0.5f, 0f, 0.35f));
    }

    // Investigation behavior: move to sound, then return to patrol.
    private void Investigate()
    {
        investigateTimer += Time.deltaTime;

        // Safety timeout: prevents getting stuck
        if (investigateTimer > 2f)
        {
            investigateTimer = 0f;
            state = EnemyState.Patrolling;
            GoToClosestPatrolPoint();
            return;
        }

        // Reached investigation point
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            investigateTimer = 0f;
            state = EnemyState.Patrolling;
            GoToClosestPatrolPoint();
        }
    }

    // -------------------------
    // SNAPSHOT RESTORE
    // -------------------------

    // Restores enemy state after checkpoint reload.
    public void RestoreSnapshot(EnemySnapshot snap)
    {
        StopAllCoroutines();

        // Restore position
        agent.Warp(snap.position);
        transform.position = snap.position;

        // Reset AI state
        state = EnemyState.Patrolling;
        isWaiting = false;
        playerInCone = false;
        chaseStarted = false;
        hasPlayedInvestigateSound = false;

        if (loseRoutine != null) StopCoroutine(loseRoutine);
        loseRoutine = null;

        // Stop all audio
        if (chaseMusic.isPlaying) chaseMusic.Stop();
        if (investigateSound.isPlaying) investigateSound.Stop();

        // Reset movement
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.ResetPath();

        // Reset cone color
        FadeConeToColor(new Color(1f, 1f, 0f, 0.25f));

        // Restore zone
        currentZoneIndex = snap.zoneIndex;

        var zones = Object.FindObjectsByType<PatrolZone>(FindObjectsSortMode.None);
        foreach (var z in zones)
        {
            if (z.zoneIndex == currentZoneIndex)
            {
                currentPatrolPoints = z.patrolPoints;
                break;
            }
        }

        // Rejoin patrol next frame
        StartCoroutine(RestorePatrolNextFrame());
    }

    // After restoring, join the closest patrol point.
    private IEnumerator RestorePatrolNextFrame()
    {
        yield return null;

        if (currentPatrolPoints != null && currentPatrolPoints.Length > 0)
        {
            int closest = 0;
            float bestDist = float.MaxValue;

            for (int i = 0; i < currentPatrolPoints.Length; i++)
            {
                float d = Vector3.Distance(transform.position, currentPatrolPoints[i].position);
                if (d < bestDist)
                {
                    bestDist = d;
                    closest = i;
                }
            }

            patrolIndex = closest;
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(currentPatrolPoints[patrolIndex].position);
        }
    }
}