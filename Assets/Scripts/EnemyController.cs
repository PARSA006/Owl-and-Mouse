using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public enum EnemyState
{
    Patrolling,
    Following,
    Attacking
}

public class NewMonoBehaviourScript : MonoBehaviour
{
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Audio")]
    [SerializeField] private AudioSource chaseMusic;

    [Header("Vision Cones")]
    [SerializeField] private Renderer[] coneRenderers;
    [SerializeField] private float coneFadeSpeed = 3f;

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseStartSpeed = 4f;
    [SerializeField] private float maxChaseSpeed = 8f;

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

    private NavMeshAgent _agent;
    private Animator _animator;
    private EnemyState _state = EnemyState.Patrolling;
    private int _currentPatrolIndex;
    private bool _isWaiting;

    private Coroutine _coneFadeRoutine;
    private Coroutine _accelRoutine;
    private Coroutine _losePlayerRoutine;

    // ⭐ Prevents chase acceleration from restarting
    private bool hasStartedChase = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _agent.speed = patrolSpeed;
        _agent.angularSpeed = turnSpeed;
        _agent.updateRotation = false;

        _agent.autoBraking = false;
        _agent.acceleration = 999f;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        _agent.stoppingDistance = 0f;

        GoToNextPatrolPoint();
        FadeConeToColor(new Color(1f, 1f, 0f, 0.25f));
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (_state)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;

            case EnemyState.Following:
                FollowPlayerManual();
                if (distanceToPlayer <= attackRange)
                {
                    _state = EnemyState.Attacking;
                    StartCoroutine(RestartAfterDelay(0.5f));
                }
                break;

            case EnemyState.Attacking:
                break;
        }

        if (_state == EnemyState.Following)
            RotateTowardPlayer();
        else
            RotateTowardMovementDirection();

        UpdateAnimations();
    }

    // -----------------------------
    // Rotation
    // -----------------------------
    private void RotateTowardPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }

    private void RotateTowardMovementDirection()
    {
        if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_agent.velocity.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }

    // -----------------------------
    // Vision Cone Detection
    // -----------------------------
    public void PlayerEnteredCone()
    {
        if (_state == EnemyState.Attacking) return;

        _state = EnemyState.Following;

        // ⭐ Only start chase acceleration ONCE
        if (!hasStartedChase)
        {
            hasStartedChase = true;

            if (_accelRoutine != null)
                StopCoroutine(_accelRoutine);

            _accelRoutine = StartCoroutine(AccelerateToChaseStartSpeed());
        }

        if (_losePlayerRoutine != null)
        {
            StopCoroutine(_losePlayerRoutine);
            _losePlayerRoutine = null;
        }

        if (!chaseMusic.isPlaying)
            chaseMusic.Play();

        FadeConeToColor(new Color(1f, 0f, 0f, 0.35f));
    }

    public void PlayerExitedCone()
    {
        if (_state != EnemyState.Following) return;

        if (_losePlayerRoutine != null)
            StopCoroutine(_losePlayerRoutine);

        _losePlayerRoutine = StartCoroutine(LosePlayerRoutine());
    }

    // -----------------------------
    // ⭐ FIXED LosePlayerRoutine
    // -----------------------------
    private IEnumerator LosePlayerRoutine()
    {
        float timer = 0f;

        while (timer < losePlayerTime)
        {
            // If player becomes visible again, cancel immediately
            if (_state == EnemyState.Following)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        // Only give up if STILL not following
        if (_state != EnemyState.Following)
        {
            _state = EnemyState.Patrolling;

            hasStartedChase = false; // ⭐ Reset chase flag

            if (chaseMusic.isPlaying)
                chaseMusic.Stop();

            FadeConeToColor(new Color(1f, 1f, 0f, 0.25f));

            _agent.speed = patrolSpeed;
            GoToClosestPatrolPoint();
        }

        _losePlayerRoutine = null;
    }

    // -----------------------------
    // Chase Acceleration
    // -----------------------------
    private IEnumerator AccelerateToChaseStartSpeed()
    {
        float startSpeed = patrolSpeed;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / accelerationTime;
            _agent.speed = Mathf.Lerp(startSpeed, chaseStartSpeed, t);
            yield return null;
        }

        _agent.speed = chaseStartSpeed;
    }

    // -----------------------------
    // Manual Chase Movement
    // -----------------------------
    private void FollowPlayerManual()
    {
        _agent.isStopped = true;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (_agent.speed < maxChaseSpeed)
            _agent.speed += chaseAccelerationRate * Time.deltaTime;

        transform.position += direction * _agent.speed * Time.deltaTime;
    }

    // -----------------------------
    // Patrol Logic
    // -----------------------------
    private void Patrol()
    {
        _agent.isStopped = false;

        if (_isWaiting) return;

        if (!_agent.pathPending && _agent.remainingDistance <= stopAtDistance)
            StartCoroutine(WaitAtPatrolPoint());
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        _isWaiting = true;
        _agent.isStopped = true;

        yield return new WaitForSeconds(patrolWaitTime);

        _agent.isStopped = false;
        GoToNextPatrolPoint();
        _isWaiting = false;
    }

    private void GoToClosestPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        _currentPatrolIndex = closestIndex;
        _agent.SetDestination(patrolPoints[_currentPatrolIndex].position);
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        _agent.SetDestination(patrolPoints[_currentPatrolIndex].position);
        _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // -----------------------------
    // Animation
    // -----------------------------
    private void UpdateAnimations()
    {
        if (_animator == null) return;

        bool isWalking = _state == EnemyState.Following
            ? true
            : _agent.velocity.sqrMagnitude > 0.01f;

        _animator.SetBool(IsWalking, isWalking);
    }

    // -----------------------------
    // Attack + Restart
    // -----------------------------
    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // -----------------------------
    // Vision Cone Fade
    // -----------------------------
    private void FadeConeToColor(Color targetColor)
    {
        if (_coneFadeRoutine != null)
            StopCoroutine(_coneFadeRoutine);

        _coneFadeRoutine = StartCoroutine(FadeConeRoutine(targetColor));
    }

    private IEnumerator FadeConeRoutine(Color targetColor)
    {
        Color[] startColors = new Color[coneRenderers.Length];

        for (int i = 0; i < coneRenderers.Length; i++)
        {
            if (coneRenderers[i] != null)
                startColors[i] = coneRenderers[i].material.GetColor("_BaseColor");
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * coneFadeSpeed;

            for (int i = 0; i < coneRenderers.Length; i++)
            {
                if (coneRenderers[i] != null)
                {
                    coneRenderers[i].material.SetColor(
                        "_BaseColor",
                        Color.Lerp(startColors[i], targetColor, t)
                    );
                }
            }

            yield return null;
        }
    }
}
