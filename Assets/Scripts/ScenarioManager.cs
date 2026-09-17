using UnityEngine;

public sealed class ScenarioManager : MonoBehaviour
{
    [SerializeField] private EnemyHealth[] base2Enemies;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private FPSPlayerController playerController;
    [SerializeField] private FPSMouseLook playerLook;
    [SerializeField] private PlayerWeapon playerWeapon;

    private bool playerInEndArea;

    public bool IsComplete { get; private set; }

    private void Awake()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);

        if (playerController == null)
            playerController = FindFirstObjectByType<FPSPlayerController>();

        if (playerLook == null)
            playerLook = FindFirstObjectByType<FPSMouseLook>();

        if (playerWeapon == null)
            playerWeapon = FindFirstObjectByType<PlayerWeapon>();
    }

    private void OnEnable()
    {
        if (base2Enemies == null)
            return;

        foreach (EnemyHealth enemy in base2Enemies)
        {
            if (enemy != null)
                enemy.HealthChanged += HandleEnemyHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (base2Enemies == null)
            return;

        foreach (EnemyHealth enemy in base2Enemies)
        {
            if (enemy != null)
                enemy.HealthChanged -= HandleEnemyHealthChanged;
        }
    }

    public void SetPlayerInEndArea(bool isInside)
    {
        playerInEndArea = isInside;

        if (playerInEndArea)
            TryComplete();
    }

    public bool TryComplete()
    {
        if (IsComplete || !playerInEndArea || !AreBase2EnemiesEliminated())
            return false;

        IsComplete = true;

        if (completionPanel != null)
            completionPanel.SetActive(true);

        if (playerController != null)
            playerController.enabled = false;

        if (playerLook != null)
            playerLook.enabled = false;

        if (playerWeapon != null)
            playerWeapon.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        return true;
    }

    private bool AreBase2EnemiesEliminated()
    {
        if (base2Enemies == null || base2Enemies.Length == 0)
            return false;

        foreach (EnemyHealth enemy in base2Enemies)
        {
            if (enemy == null || !enemy.IsDead)
                return false;
        }

        return true;
    }

    private void HandleEnemyHealthChanged(float currentHealth, float maxHealth)
    {
        if (playerInEndArea)
            TryComplete();
    }
}
