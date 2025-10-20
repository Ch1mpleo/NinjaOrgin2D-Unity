using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public static event Action OnEnemyDeadEvent;
    
    [Header("Config")]
    [SerializeField] private float health;

    public float CurrentHealth { get; private set; }

    private Animator animator;
    private Rigidbody2D rb2D;
    private EnemyBrain enemyBrain;
    private EnemyLoot enemyLoot;
    private EnemySelector enemySelector;
    
    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyLoot = GetComponent<EnemyLoot>();
        enemyBrain = GetComponent<EnemyBrain>();
        enemySelector = GetComponent<EnemySelector>();
    }

    private void Start()
    {
        CurrentHealth = health;
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0f)
        {
            DisableEnemy();
            QuestManager.Instance.AddProgress("Kill2Enemy", 1); 
            QuestManager.Instance.AddProgress("Kill5Enemy", 1);
            QuestManager.Instance.AddProgress("Kill10Enemy", 1);
        }
        else
        {
            DamageManager.Instance.ShowDamageText(amount, transform);
        }
    }

    private void DisableEnemy()
    {
        animator.SetTrigger("Dead");
        enemyBrain.enabled = false;
        enemySelector.NoSelectionCallback();
        rb2D.bodyType = RigidbodyType2D.Static;
        OnEnemyDeadEvent?.Invoke();
        GameManager.Instance.AddPlayerExp(enemyLoot.ExpDrop);
    }
}