using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private InputActionReference fireAction;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private BulletProjectile projectilePrefab;
    [SerializeField] private GameObject environmentImpactPrefab;
    [SerializeField] private GameObject enemyImpactPrefab;

    [Header("Weapon")]
    [SerializeField] private float damage = 34f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float fireInterval = 0.15f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Animation Synchronization")]
    [SerializeField, Range(0f, 1f)]
    private float fireReleaseNormalizedTime = 4f / 35f;

    private static readonly int FireParameter = Animator.StringToHash("Fire");
    private static readonly int FireState = Animator.StringToHash("Fire");

    private float nextFireTime;
    private bool fireRequested;
    private bool waitingForFireRestart;

    private void OnEnable()
    {
        if (fireAction != null)
            fireAction.action.Enable();
    }

    private void OnDisable()
    {
        if (fireAction != null)
            fireAction.action.Disable();

        fireRequested = false;
    }

    private void Update()
    {
        ReleasePendingShotAtAnimationMoment();

        if (fireAction != null && fireAction.action.IsPressed())
            TryBeginFire();
    }

    private void TryBeginFire()
    {
        if (fireRequested || Time.time < nextFireTime || playerCamera == null ||
            playerAnimator == null ||
            muzzlePoint == null || projectilePrefab == null)
            return;

        nextFireTime = Time.time + fireInterval;
        fireRequested = true;
        waitingForFireRestart =
            playerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == FireState;

        playerAnimator.SetTrigger(FireParameter);
    }

    private void ReleasePendingShotAtAnimationMoment()
    {
        if (!fireRequested || playerAnimator == null)
            return;

        AnimatorStateInfo fireState = playerAnimator.GetCurrentAnimatorStateInfo(0);
        bool transitioningIntoFire = false;

        if (playerAnimator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = playerAnimator.GetNextAnimatorStateInfo(0);

            if (nextState.shortNameHash == FireState)
            {
                fireState = nextState;
                transitioningIntoFire = true;
            }
        }

        if (fireState.shortNameHash != FireState)
            return;

        if (waitingForFireRestart)
        {
            if (!transitioningIntoFire &&
                fireState.normalizedTime >= fireReleaseNormalizedTime)
                return;

            waitingForFireRestart = false;
        }

        if (fireState.normalizedTime < fireReleaseNormalizedTime)
            return;

        fireRequested = false;
        FireProjectile();
    }

    private void FireProjectile()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play(true);

        Vector3 aimPoint = GetAimPoint();
        Vector3 direction = (aimPoint - muzzlePoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        BulletProjectile projectile = Instantiate(
            projectilePrefab,
            muzzlePoint.position,
            rotation);

        projectile.Initialize(
            damage,
            gameObject,
            environmentImpactPrefab,
            enemyImpactPrefab);
    }

    private Vector3 GetAimPoint()
    {
        Ray aimRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(
                aimRay,
                out RaycastHit hit,
                range,
                hitMask,
                QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return aimRay.origin + aimRay.direction * range;
    }
}
