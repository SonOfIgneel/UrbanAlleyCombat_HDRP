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
    [SerializeField] private GameObject environmentImpactPrefab;
    [SerializeField] private GameObject enemyImpactPrefab;

    [Header("Weapon")]
    [SerializeField] private float damage = 34f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float fireInterval = 0.15f;
    [SerializeField] private LayerMask hitMask = ~0;

    private static readonly int FireParameter = Animator.StringToHash("Fire");

    private float nextFireTime;

    private void OnEnable()
    {
        if (fireAction != null)
            fireAction.action.Enable();
    }

    private void OnDisable()
    {
        if (fireAction != null)
            fireAction.action.Disable();
    }

    private void Update()
    {
        if (fireAction == null || !fireAction.action.IsPressed())
            return;

        TryFire();
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime || playerCamera == null)
            return;

        nextFireTime = Time.time + fireInterval;

        if (playerAnimator != null)
            playerAnimator.SetTrigger(FireParameter);

        if (muzzleFlash != null)
            muzzleFlash.Play(true);

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            return;

        EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            SpawnImpact(enemyImpactPrefab, hit);
            return;
        }

        SpawnImpact(environmentImpactPrefab, hit);
    }

    private static void SpawnImpact(GameObject prefab, RaycastHit hit)
    {
        if (prefab == null)
            return;

        Quaternion rotation = Quaternion.LookRotation(hit.normal);
        Instantiate(prefab, hit.point + hit.normal * 0.01f, rotation);
    }
}
