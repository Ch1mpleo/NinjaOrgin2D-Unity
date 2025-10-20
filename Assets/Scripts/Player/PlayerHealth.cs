using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats;

    private PlayerAnimations playerAnimations;
    public bool PlayerDead { get; private set; } = false; // Thay isDead b?ng PlayerDead

    private void Awake()
    {
        playerAnimations = GetComponent<PlayerAnimations>();
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
        ShowReviveQuestion();
    }

    public void Revive()
    {
        stats.Health = stats.MaxHealth;
        PlayerDead = false;
        playerAnimations.ResetPlayer();
        gameObject.SetActive(true);
    }

    private void ShowReviveQuestion()
    {
        ReviveQuestion[] questions = new ReviveQuestion[]
        {
            new ReviveQuestion { QuestionText = "2 + 2 b?ng m?y?", CorrectAnswer = "4" },
            new ReviveQuestion { QuestionText = "Th? ?ô c?a Vi?t Nam là gì?", CorrectAnswer = "Hà N?i" },
            new ReviveQuestion { QuestionText = "Màu c?a lá cây là gì?", CorrectAnswer = "Xanh" }
        };

        int randomIndex = Random.Range(0, questions.Length);
        ReviveQuestion question = questions[randomIndex];

        Object.FindAnyObjectByType<RevivePanel>()?.Show(question, this);
    }
}