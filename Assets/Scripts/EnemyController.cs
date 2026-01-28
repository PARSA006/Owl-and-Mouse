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
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Audio")]
    [SerializeField] private AudioSource chaseMusic;
    [SerializeField] private AudioSource investigateSound;   // NEW

    [Header("Vision Cone Fade")]
    [SerializeField] private Renderer[] coneRenderers;
    [SerializeField] private float coneFadeSpeed = 3f;

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseStartSpeed = 4f;
    [SerializeField] private float maxChaseSpeed = 8f;

    [Header("Investigation Settings")]
    [SerializeField] private float investigateSpeed = 3f;   // NEW

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

    private bool hasPlayedInvestigateSound = false;   // NEW

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
        agent.stoppingDistance = 0f;

        GoToNextPatrolPoint();
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
                    PlayerRespawn.RespawnPlayer();
                }
                break;

            case EnemyState.Investigating:
                Investigate();
                break;
        }

        // Updated rotation logic
        if (state == EnemyState.Following)
            RotateTowardPlayer();
        else if (state == EnemyState.Patrolling || state == EnemyState.Investigating)
            RotateTowardMovementDirection();

        UpdateAnimations();
    }

    private void TryFindPlayer()
    {
        var playerObj = FindFirstObjectByType<PlayerMovement>();
        if (playerObj != null)
            player = playerObj.transform;
    }

    // -----------------------------
    // Rotation
    // -----------------------------
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

    // -----------------------------
    // Detection (called by SkyLightCone)
    // -----------------------------
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

    // -----------------------------
    // Player exited cone
    // -----------------------------
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

    // -----------------------------
    // Chase Acceleration
    // -----------------------------
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

    // -----------------------------
    // Chase using NavMeshAgent
    // -----------------------------
    private void FollowPlayer()
    {
        if (player == null) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (agent.speed < maxChaseSpeed)
            agent.speed += chaseAccelerationRate * Time.deltaTime;
    }

    // -----------------------------
    // Patrol Logic
    // -----------------------------
    private void Patrol()
    {
        agent.isStopped = false;

        if (isWaiting) return;

        if (!agent.pathPending && agent.remainingDistance <= stopAtDistance)
            StartCoroutine(WaitAtPatrolPoint());
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(patrolWaitTime);

        agent.isStopped = false;
        GoToNextPatrolPoint();
        isWaiting = false;
    }

    private void GoToClosestPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        int closest = 0;
        float bestDist = float.MaxValue;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float d = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                closest = i;
            }
        }

        patrolIndex = closest;
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[patrolIndex].position);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    // -----------------------------
    // Animation
    // -----------------------------
    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool walking =
            state == EnemyState.Following ||
            state == EnemyState.Investigating ||
            agent.velocity.sqrMagnitude > 0.01f;

        animator.SetBool(IsWalking, walking);
    }

    // -----------------------------
    // Vision Cone Fade
    // -----------------------------
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

    // -----------------------------
    // Sound Investigation Trigger
    // -----------------------------
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

    // -----------------------------
    // Investigating Behavior
    // -----------------------------
    private void Investigate()
    {
        if (!agent.pathPending && agent.remainingDistance <= stopAtDistance)
        {
            state = EnemyState.Patrolling;
            chaseStarted = false;

            hasPlayedInvestigateSound = false;

            if (chaseMusic.isPlaying)
                chaseMusic.Stop();

            FadeConeToColor(new Color(1f, 1f, 0f, 0.25f));

            GoToClosestPatrolPoint();
        }
    }
}
