using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Canvas healthCanvas;
    [SerializeField] private Image healthFill;
    [SerializeField] private Text healthPercentText;
    [SerializeField, Min(0f)] private float smoothDuration = 0.15f;
    [SerializeField, Min(0f)] private float maxVisibleDistance = 35f;

    [Header("Health Colors")]
    [SerializeField] private Color healthyColor = new Color(0.2f, 0.85f, 0.3f);
    [SerializeField] private Color warningColor = new Color(1f, 0.65f, 0.1f);
    [SerializeField] private Color criticalColor = new Color(0.95f, 0.16f, 0.12f);

    private Camera targetCamera;
    private Transform followTarget;
    private float worldHeightOffset;
    private float targetFill = 1f;

    private void Awake()
    {
        if (healthCanvas == null)
            healthCanvas = GetComponent<Canvas>();

        targetCamera = Camera.main;

        if (enemyHealth != null)
        {
            followTarget = enemyHealth.transform;
            worldHeightOffset = transform.position.y - followTarget.position.y;
        }
    }

    private void OnEnable()
    {
        if (enemyHealth == null)
            return;

        enemyHealth.HealthChanged += HandleHealthChanged;
        Refresh(enemyHealth.CurrentHealth, enemyHealth.MaxHealth, true);
    }

    private void Start()
    {
        if (enemyHealth != null)
            Refresh(enemyHealth.CurrentHealth, enemyHealth.MaxHealth, true);
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.HealthChanged -= HandleHealthChanged;
    }

    private void Update()
    {
        if (healthFill == null || Mathf.Approximately(healthFill.fillAmount, targetFill))
            return;

        if (smoothDuration <= 0f)
        {
            healthFill.fillAmount = targetFill;
            return;
        }

        healthFill.fillAmount = Mathf.MoveTowards(
            healthFill.fillAmount,
            targetFill,
            Time.deltaTime / smoothDuration);
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
                return;
        }

        if (followTarget != null)
            transform.position = followTarget.position + Vector3.up * worldHeightOffset;

        Vector3 lookDirection = transform.position - targetCamera.transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);

        if (healthCanvas != null && maxVisibleDistance > 0f)
        {
            float distance = Vector3.Distance(transform.position, targetCamera.transform.position);
            healthCanvas.enabled = distance <= maxVisibleDistance;
        }
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (currentHealth <= 0f || enemyHealth.IsDead)
        {
            gameObject.SetActive(false);
            return;
        }

        Refresh(currentHealth, maxHealth, false);
    }

    private void Refresh(float currentHealth, float maxHealth, bool immediate)
    {
        targetFill = maxHealth > 0f
            ? Mathf.Clamp01(currentHealth / maxHealth)
            : 0f;

        if (healthFill != null)
        {
            if (immediate)
                healthFill.fillAmount = targetFill;

            healthFill.color = GetHealthColor(targetFill);
        }

        if (healthPercentText != null)
            healthPercentText.text = Mathf.RoundToInt(targetFill * 100f) + "%";
    }

    private Color GetHealthColor(float healthRatio)
    {
        if (healthRatio > 0.6f)
            return healthyColor;

        return healthRatio >= 0.3f ? warningColor : criticalColor;
    }
}
