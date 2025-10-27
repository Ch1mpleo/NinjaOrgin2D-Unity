using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats;

    [Header("Revive")]
    [SerializeField] private RevivePanel revivePanel; // assign in inspector if possible
    [SerializeField] private int reviveAttempts = 3;

    private PlayerAnimations playerAnimations;
    public bool PlayerDead { get; private set; } = false;

    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;

    // Cached reference để tránh FindObjectOfType lặp lại
    private ReviveQuest reviveQuest;

    private void Awake()
    {
        playerAnimations = GetComponent<PlayerAnimations>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();

        // Cache ReviveQuest reference
        reviveQuest = ReviveQuest.Instance;
        if (reviveQuest == null)
        {
            reviveQuest = UnityEngine.Object.FindAnyObjectByType<ReviveQuest>();
            if (reviveQuest == null)
            {
                Debug.LogWarning("PlayerHealth: ReviveQuest không tìm thấy trong scene. Chức năng revive có thể không hoạt động!");
            }
        }
    }

    private void Update()
    {
        if (stats.Health <= 0f && !PlayerDead)
        {
            SetPlayerDead();
        }
    }

    public void TakeDamage(float amount)
    {
        if (stats.Health <= 0f) return;
        stats.Health -= amount;
        DamageManager.Instance.ShowDamageText(amount, transform);
        if (stats.Health <= 0f && !PlayerDead)
        {
            stats.Health = 0f;
            SetPlayerDead();
        }
    }

    public void RestoreHealth(float amount)
    {
        stats.Health += amount;
        if (stats.Health > stats.MaxHealth)
        {
            stats.Health = stats.MaxHealth;
        }
    }

    public bool CanRestoreHealth()
    {
        return stats.Health > 0 && stats.Health < stats.MaxHealth;
    }

    private void SetPlayerDead()
    {
        PlayerDead = true;
        playerAnimations.SetDeadAnimation();

        // Disable movement and attack components so player cannot act while dead
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerAttack != null) playerAttack.enabled = false;

        ShowReviveQuestion();
    }

    public void Revive()
    {
        stats.Health = stats.MaxHealth;
        PlayerDead = false;
        playerAnimations.ResetPlayer();

        // Re-enable components
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerAttack != null) playerAttack.enabled = true;

        // Ensure GameObject is active (in case external logic deactivated it on fail)
        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }

    private void ShowReviveQuestion()
    {
        // Lấy câu hỏi từ ReviveQuest thay vì hardcode
        if (reviveQuest == null)
        {
            // Thử tìm lại nếu chưa có
            reviveQuest = ReviveQuest.Instance ?? UnityEngine.Object.FindAnyObjectByType<ReviveQuest>();

            if (reviveQuest == null)
            {
                Debug.LogError("PlayerHealth: Không tìm thấy ReviveQuest trong scene. Không thể hiển thị câu hỏi revive!");
                HandleReviveFailed();
                return;
            }
        }

        // Lấy câu hỏi ngẫu nhiên từ ReviveQuest
        ReviveQuestion question = reviveQuest.GetRandomQuestion();

        if (question == null)
        {
            Debug.LogError("PlayerHealth: Không thể lấy câu hỏi từ ReviveQuest!");
            HandleReviveFailed();
            return;
        }

        // Use assigned revivePanel if available, otherwise try to find one in scene
        RevivePanel panelToUse = revivePanel;
        if (panelToUse == null)
        {
            panelToUse = UnityEngine.Object.FindAnyObjectByType<RevivePanel>();
        }

        if (panelToUse != null)
        {
            panelToUse.Show(question, this, reviveAttempts, HandleReviveFailed);
        }
        else
        {
            Debug.LogWarning("PlayerHealth: RevivePanel không tìm thấy trong scene. Player sẽ chết.");
            HandleReviveFailed();
        }
    }

    private void HandleReviveFailed()
    {
        // Simple fail behavior: deactivate player GameObject (game over).
        // You can replace this with a more advanced game over flow.
        Debug.Log("Player failed revive attempts. Game over.");
        gameObject.SetActive(false);
    }
}