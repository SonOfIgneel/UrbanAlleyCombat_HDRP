using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFill;
    [SerializeField] private Text healthPercentText;
    [SerializeField, Min(0f)] private float smoothDuration = 0.15f;

    [Header("Health Colors")]
    [SerializeField] private Color healthyColor = new Color(0.2f, 0.85f, 0.3f);
    [SerializeField] private Color warningColor = new Color(1f, 0.65f, 0.1f);
    [SerializeField] private Color criticalColor = new Color(0.95f, 0.16f, 0.12f);

    private float targetFill = 1f;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth == null)
            return;

        playerHealth.HealthChanged += HandleHealthChanged;
        Refresh(playerHealth.CurrentHealth, playerHealth.MaxHealth, true);
    }

    private void Start()
    {
        if (playerHealth != null)
            Refresh(playerHealth.CurrentHealth, playerHealth.MaxHealth, true);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= HandleHealthChanged;
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
            Time.unscaledDeltaTime / smoothDuration);
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
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
