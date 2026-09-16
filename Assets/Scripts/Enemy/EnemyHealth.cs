using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Collider combatCollider;
    [SerializeField] private float colliderDisableDelay = 1.2f;

    private static readonly int HitParameter = Animator.StringToHash("Hit");
    private static readonly int IsDeadParameter = Animator.StringToHash("IsDead");

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || IsDead)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        enemyAI?.Alert();

        if (CurrentHealth <= 0f)
        {
            Die();
            return;
        }

        if (animator != null)
            animator.SetTrigger(HitParameter);
    }

    private void Die()
    {
        IsDead = true;

        if (animator != null)
            animator.SetBool(IsDeadParameter, true);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (enemyAI != null)
        {
            enemyAI.Die();
            enemyAI.enabled = false;
        }

        if (combatCollider != null)
            StartCoroutine(DisableColliderAfterDelay());
    }

    private IEnumerator DisableColliderAfterDelay()
    {
        yield return new WaitForSeconds(colliderDisableDelay);
        combatCollider.enabled = false;
    }
}
