using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats;
    [Header("Revive")]
    [SerializeField] private RevivePanel revivePanel; // assign in inspector if possible
    [SerializeField] private int reviveAttempts = 3;

    private PlayerAnimations playerAnimations;
    public bool PlayerDead { get; private set; } = false; // Thay isDead b?ng PlayerDead

    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        playerAnimations = GetComponent<PlayerAnimations>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
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
        // Ensure this source file is saved with UTF-8 encoding in your editor so Vietnamese characters are preserved.
        ReviveQuestion[] questions = new ReviveQuestion[]
        {
            new ReviveQuestion { QuestionText = "2 + 2 b?ng m?y?", CorrectAnswer = "4" },
            new ReviveQuestion { QuestionText = "Th? ?ô c?a Vi?t Nam là gì?", CorrectAnswer = "Hà N?i" },
            new ReviveQuestion { QuestionText = "Màu c?a lá cây là gì?", CorrectAnswer = "Xanh" }
        };

        int randomIndex = UnityEngine.Random.Range(0, questions.Length);
        ReviveQuestion question = questions[randomIndex];

        // Use assigned revivePanel if available, otherwise try to find one in scene
        RevivePanel panelToUse = revivePanel ?? UnityEngine.Object.FindAnyObjectByType<RevivePanel>();
        if (panelToUse != null)
        {
            panelToUse.Show(question, this, reviveAttempts, HandleReviveFailed);
        }
        else
        {
            Debug.LogWarning("RevivePanel not found in scene. Player will remain dead.");
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