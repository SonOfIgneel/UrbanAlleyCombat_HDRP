using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 80f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float damage = 34f;
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private GameObject characterImpactPrefab;

    private Rigidbody body;
    private GameObject owner;
    private bool hasImpacted;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }

    public void Initialize(
        float newDamage,
        GameObject newOwner,
        GameObject impactOverride = null,
        GameObject characterImpactOverride = null)
    {
        damage = newDamage;
        owner = newOwner;

        if (impactOverride != null)
            impactPrefab = impactOverride;

        if (characterImpactOverride != null)
            characterImpactPrefab = characterImpactOverride;

        IgnoreOwnerCollisions();
        body.linearVelocity = transform.forward * speed;
    }

    private void IgnoreOwnerCollisions()
    {
        if (owner == null)
            return;

        Collider projectileCollider = GetComponent<Collider>();
        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>(true);

        foreach (Collider ownerCollider in ownerColliders)
            Physics.IgnoreCollision(projectileCollider, ownerCollider, true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted)
            return;

        if (owner != null && collision.transform.IsChildOf(owner.transform))
            return;

        hasImpacted = true;

        EnemyHealth enemyHealth = collision.collider.GetComponentInParent<EnemyHealth>();
        PlayerHealth playerHealth = collision.collider.GetComponentInParent<PlayerHealth>();

        if (enemyHealth != null)
            enemyHealth.TakeDamage(damage);
        else if (playerHealth != null)
            playerHealth.TakeDamage(damage);

        ContactPoint contact = collision.GetContact(0);
        GameObject selectedImpact = enemyHealth != null || playerHealth != null
            ? characterImpactPrefab
            : impactPrefab;

        if (selectedImpact != null)
        {
            Quaternion rotation = Quaternion.LookRotation(contact.normal);
            Instantiate(selectedImpact, contact.point + contact.normal * 0.01f, rotation);
        }

        Destroy(gameObject);
    }
}
