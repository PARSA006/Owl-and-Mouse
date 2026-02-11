using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public enum EnemyState
{
    Patrolling,
    Following,
    Attacking,
    Investigating
}

public class NewMonoBehaviourScript : MonoBehaviour
{
    private float investigateTimer = 0f;

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Default Patrol Points (Zone 0)")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Audio")]
    [SerializeField] private AudioSource chaseMusic;
    [SerializeField] private AudioSource investigateSound;

    [Header("Vision Cone Fade")]
    [SerializeField] private Renderer[] coneRenderers;
    [SerializeField] private float coneFadeSpeed = 3f;

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseStartSpeed = 4f;
    [SerializeField] private float maxChaseSpeed = 8f;

    [Header("Investigation Settings")]
    [SerializeField] private float investigateSpeed = 3f;

    [Header("Chase Acceleration")]
    [SerializeField] private float accelerationTime = 1.5f;
    [SerializeField] private float chaseAccelerationRate = 0.5f;

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 720f;

    [Header("Settings")]
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float stopAtDistance = 0.5f;
    [SerializeField] private float losePlayerTime = 3f;
    [SerializeField] private float attackRange = 1.2f;

    private NavMeshAgent agent;
    private Animator animator;
    private EnemyState state = EnemyState.Patrolling;

    private int patrolIndex;
    private bool isWaiting;

    private Coroutine fadeRoutine;
    private Coroutine accelRoutine;
    private Coroutine loseRoutine;

    private bool chaseStarted = false;
    private bool playerInCone = false;
    private bool hasPlayedInvestigateSound = false;

    // -------------------------
    // ZONE SYSTEM
    // -------------------------
    public int currentZoneIndex = 0;
    public Transform[] currentPatrolPoints;

    public void SwitchToZone(int zoneIndex, Transform[] newPoints)
    {
        currentZoneIndex = zoneIndex;
        currentPatrolPoints = newPoints;

        if (currentPatrolPoints == null || currentPatrolPoints.Length == 0)
            return;

        // Join the new zone at the closest patrol point
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
        StartCoroutine(SetDestinationNextFrame(currentPatrolPoints[patrolIndex].position));

        Debug.Log("Enemy switched to zone " + zoneIndex);
    }

    private IEnumerator SetDestinationNextFrame(Vector3 pos)
    {
        yield return null;
        agent.SetDestination(pos);
    }

    // -------------------------

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedPlayerFind());
    }

    private IEnumerator DelayedPlayerFind()
    {
        yield return null;
        TryFindPlayer();
    }

    private void Start()
    {
        TryFindPlayer();

        agent.speed = patrolSpeed;
        agent.angularSpeed = turnSpeed;
        agent.updateRotation = false;
        agent.autoBraking = false;
        agent.acceleration = 999f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.stoppingDistance = stopAtDistance;

        // Default zone = 0
        currentPatrolPoints = patrolPoints;

        if (!PlayerRespawn.restoredFromCheckpoint)
        {
            patrolIndex = 0;
            GoToNextPatrolPoint();
        }
        else
        {
            // On checkpoint restore we’ll rejoin patrol via RestoreSnapshot
        }

        FadeConeToColor(new Color(1f, 1f, 0f, 0.25f));
    }

    private void Update()
    {
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

        if (state == EnemyState.Following)
            RotateTowardPlayer();
        else
            RotateTowardMovementDirection();

        UpdateAnimations();
    }

    private void TryFindPlayer()
    {
        var playerObj = FindFirstObjectByType<PlayerMovement>();
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void RotateTowardPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
        }
    }

    private void RotateTowardMovementDirection()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion target = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
        }
    }

    public void PlayerEnteredCone()
    {
        playerInCone = true;

        if (state == EnemyState.Attacking) return;

        state = EnemyState.Following;

        if (!chaseStarted)
        {
            chaseStarted = true;

            if (accelRoutine != null)
                StopCoroutine(accelRoutine);

            accelRoutine = StartCoroutine(AccelerateToChaseStartSpeed());
        }

        if (loseRoutine != null)
        {
            StopCoroutine(loseRoutine);
            loseRoutine = null;
        }

        if (!chaseMusic.isPlaying)
            chaseMusic.Play();

        FadeConeToColor(new Color(1f, 0f, 0f, 0.35f));
    }

    public void PlayerExitedCone()
    {
        playerInCone = false;

        if (state != EnemyState.Following) return;

        if (loseRoutine != null)
            StopCoroutine(loseRoutine);

        loseRoutine = StartCoroutine(LosePlayerRoutine());
    }

    private IEnumerator LosePlayerRoutine()
    {
        float timer = 0f;

        while (timer < losePlayerTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

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

    private void FollowPlayer()
    {
        if (player == null) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (agent.speed < maxChaseSpeed)
            agent.speed += chaseAccelerationRate * Time.deltaTime;
    }

    private void Patrol()
    {
        investigateTimer = 0f;


        agent.isStopped = false;

        // Safety: if something went wrong with the path, rebuild it
        if (agent.hasPath && float.IsInfinity(agent.remainingDistance))
        {
            agent.ResetPath();
            agent.SetDestination(currentPatrolPoints[patrolIndex].position);
            return;
        }

        if (isWaiting)
            return;

        // Use stoppingDistance to detect arrival
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(patrolWaitTime);

        patrolIndex = (patrolIndex + 1) % currentPatrolPoints.Length;

        agent.isStopped = false;
        agent.ResetPath();
        yield return null;

        agent.SetDestination(currentPatrolPoints[patrolIndex].position);

        isWaiting = false;
    }

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

    private void GoToNextPatrolPoint()
    {
        if (currentPatrolPoints == null || currentPatrolPoints.Length == 0) return;
        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(currentPatrolPoints[patrolIndex].position);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool walking =
            state == EnemyState.Following ||
            state == EnemyState.Investigating ||
            agent.velocity.sqrMagnitude > 0.01f;

        animator.SetBool(IsWalking, walking);
    }

    private void FadeConeToColor(Color target)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeConeRoutine(target));
    }

    private IEnumerator FadeConeRoutine(Color target)
    {
        Color[] start = new Color[coneRenderers.Length];

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

    public void InvestigateSound(Vector3 soundPosition)
    {
        state = EnemyState.Investigating;
        playerInCone = false;

        agent.speed = investigateSpeed;

        if (loseRoutine != null)
        {
            StopCoroutine(loseRoutine);
            loseRoutine = null;
        }

        agent.isStopped = false;
        agent.SetDestination(soundPosition);

        if (!hasPlayedInvestigateSound && investigateSound != null)
        {
            investigateSound.Play();
            hasPlayedInvestigateSound = true;
        }

        FadeConeToColor(new Color(1f, 0.5f, 0f, 0.35f));
    }

    private void Investigate()
    {
        investigateTimer += Time.deltaTime;

        // Safety timeout: never get stuck in Investigating
        if (investigateTimer > 2f)
        {
            investigateTimer = 0f;
            state = EnemyState.Patrolling;
            GoToClosestPatrolPoint();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            investigateTimer = 0f;
            state = EnemyState.Patrolling;
            GoToClosestPatrolPoint();
        }
    }


    // -------------------------
    // SNAPSHOT RESTORE (SIMPLIFIED)
    // -------------------------
    public void RestoreSnapshot(EnemySnapshot snap)
    {
        StopAllCoroutines();

        // Position
        agent.Warp(snap.position);
        transform.position = snap.position;

        // Force clean state after respawn
        state = EnemyState.Patrolling;
        isWaiting = false;
        playerInCone = false;
        chaseStarted = false;
        hasPlayedInvestigateSound = false;

        if (loseRoutine != null) StopCoroutine(loseRoutine);
        loseRoutine = null;


        if (chaseMusic.isPlaying) chaseMusic.Stop();
        if (investigateSound.isPlaying) investigateSound.Stop();

        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.ResetPath();

        FadeConeToColor(new Color(1f, 1f, 0f, 0.25f));

        // Restore zone patrol points
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

        // Rejoin patrol via closest point in that zone
        StartCoroutine(RestorePatrolNextFrame());
    }

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
