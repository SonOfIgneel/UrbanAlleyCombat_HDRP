using System;
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
    public float MaxHealth => maxHealth;
    public bool IsDead { get; private set; }
    public event Action<float, float> HealthChanged;

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
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            return;
        }

        if (animator != null)
            animator.SetTrigger(HitParameter);

        HealthChanged?.Invoke(CurrentHealth, maxHealth);
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
