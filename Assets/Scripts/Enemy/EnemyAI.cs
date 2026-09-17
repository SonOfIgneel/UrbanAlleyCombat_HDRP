using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator animator;
    [SerializeField] private BulletProjectile projectilePrefab;
    [SerializeField] private ParticleSystem muzzleFlash;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 22f;
    [SerializeField, Range(1f, 360f)] private float fieldOfView = 120f;
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private LayerMask lineOfSightMask = ~0;

    [Header("Combat")]
    [SerializeField] private float attackRange = 16f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float fireInterval = 1f;
    [SerializeField] private float turnSpeed = 8f;

    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int HasTargetParameter = Animator.StringToHash("HasTarget");
    private static readonly int FireParameter = Animator.StringToHash("Fire");

    private NavMeshAgent agent;
    private Transform player;
    private Vector3 lastKnownPosition;
    private float nextFireTime;
    private bool alerted;

    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        FindPlayer();
        SetState(EnemyState.Idle);
    }

    private void Update()
    {
        if (CurrentState == EnemyState.Dead)
            return;

        if (player == null)
        {
            FindPlayer();
            UpdateAnimation();
            return;
        }

        bool canSeePlayer = CanSeePlayer(detectionRadius);
        if (canSeePlayer)
        {
            alerted = true;
            lastKnownPosition = player.position;
        }

        if (!alerted)
        {
            SetState(EnemyState.Idle);
        }
        else
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange && CanSeePlayer(attackRange))
                AttackPlayer();
            else
                MoveToLastKnownPosition();
        }

        UpdateAnimation();
    }

    public void Alert()
    {
        if (CurrentState == EnemyState.Dead)
            return;

        FindPlayer();
        alerted = true;

        if (player != null)
            lastKnownPosition = player.position;
    }

    public void Die()
    {
        CurrentState = EnemyState.Dead;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void FindPlayer()
    {
        FPSPlayerController playerController = FindFirstObjectByType<FPSPlayerController>();
        if (playerController == null)
            return;

        player = playerController.transform;
    }

    private bool CanSeePlayer(float range)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up;
        Vector3 direction = target - origin;

        if (direction.sqrMagnitude > range * range)
            return false;

        Vector3 flatDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
        if (flatDirection.sqrMagnitude > 0.001f &&
            Vector3.Angle(transform.forward, flatDirection) > fieldOfView * 0.5f)
            return false;

        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, range, lineOfSightMask, QueryTriggerInteraction.Ignore))
            return false;

        return hit.transform.GetComponentInParent<FPSPlayerController>() != null;
    }

    private void MoveToLastKnownPosition()
    {
        SetState(EnemyState.Chasing);

        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.SetDestination(lastKnownPosition);
    }

    private void AttackPlayer()
    {
        SetState(EnemyState.Attacking);

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        FacePlayer();

        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + fireInterval;

        if (animator != null)
            animator.SetTrigger(FireParameter);

        FireProjectile();
    }

    private void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void FireProjectile()
    {
        if (firePoint == null || player == null || projectilePrefab == null)
            return;

        Vector3 target = player.position + Vector3.up;
        Vector3 direction = target - firePoint.position;
        Quaternion shotRotation = Quaternion.LookRotation(direction);

        if (muzzleFlash != null)
        {
            muzzleFlash.transform.rotation = shotRotation;
            muzzleFlash.Play(true);
        }

        BulletProjectile projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            shotRotation);

        projectile.Initialize(damage, gameObject);
    }

    private void SetState(EnemyState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;

        if (animator != null)
            animator.SetBool(HasTargetParameter, alerted);
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        float speed = agent != null && agent.isOnNavMesh ? agent.velocity.magnitude : 0f;
        animator.SetFloat(SpeedParameter, speed, 0.1f, Time.deltaTime);
        animator.SetBool(HasTargetParameter, alerted);
    }
}
