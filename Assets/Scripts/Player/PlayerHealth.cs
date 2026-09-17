using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0f;
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

        HealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (IsDead)
            Debug.Log("Player eliminated.", this);
    }
}
