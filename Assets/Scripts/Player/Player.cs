using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats;

    [Header("Test")] 
    public ItemHealthPotion HealthPotion;
    public ItemManaPotion ManaPotion;
    
    public PlayerStats Stats => stats;
    public PlayerMana PlayerMana { get; private set; }
    public PlayerHealth PlayerHealth { get; private set; }
    public PlayerAttack PlayerAttack { get; private set; }
    
    private PlayerAnimations animations;

    private void Awake()
    {
        PlayerMana = GetComponent<PlayerMana>();
        PlayerHealth = GetComponent<PlayerHealth>();
        PlayerAttack = GetComponent<PlayerAttack>();
        animations = GetComponent<PlayerAnimations>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (HealthPotion.UseItem())
            {
                Debug.Log("Using Health Potion");
            }
            
            if (ManaPotion.UseItem())
            {
                Debug.Log("Using Mana Potion");
            }
        }
    }

    public void ResetPlayer()
    {
        stats.ResetPlayer();
        animations.ResetPlayer();
        PlayerMana.ResetMana();
    }

    // Apply a persisted player profile to this Player (map values into PlayerStats and runtime components)
    public void ApplyProfile(PlayerProfile profile)
    {
        if (profile == null) return;

        if (stats != null)
        {
            stats.Level = profile.Level;

            stats.MaxHealth = profile.MaxHealth;
            stats.Health = profile.Health;

            stats.MaxMana = profile.MaxMana;
            stats.Mana = profile.Mana;

            stats.CurrentExp = profile.CurrentExp;
            stats.NextLevelExp = profile.NextLevelExp;
            stats.InitialNextLevelExp = profile.InitialNextLevelExp;
            stats.ExpMultiplier = profile.ExpMultiplier;

            stats.BaseDamage = profile.BaseDamage;
            stats.CriticalChance = profile.CriticalChance;
            stats.CriticalDamage = profile.CriticalDamage;

            stats.Strength = profile.Strength;
            stats.Dexterity = profile.Dexterity;
        }

        // Ensure runtime components reflect the profile values
        if (PlayerMana != null)
        {
            PlayerMana.SetCurrentMana(profile.Mana);
        }

        // PlayerHealth reads from stats.Health; no extra call required.
        // If the profile represents a dead player (health <= 0) the PlayerHealth logic will handle it in its Update loop.
    }

    // Produce a PlayerProfile from this Player instance (used to persist changes)
    public PlayerProfile ToProfile()
    {
        var p = new PlayerProfile();
        // try to set Id/Username from current authenticated user when available
        if (AuthManager.Instance != null && AuthManager.Instance.CurrentUser != null)
        {
            p.Id = AuthManager.Instance.CurrentUser.PlayerId;
            p.Username = AuthManager.Instance.CurrentUser.Username;
        }

        if (stats != null)
        {
            p.Level = stats.Level;

            p.MaxHealth = stats.MaxHealth;
            p.Health = stats.Health;

            p.MaxMana = stats.MaxMana;
            p.Mana = stats.Mana;

            p.CurrentExp = stats.CurrentExp;
            p.NextLevelExp = stats.NextLevelExp;
            p.InitialNextLevelExp = stats.InitialNextLevelExp;
            p.ExpMultiplier = stats.ExpMultiplier;

            p.BaseDamage = stats.BaseDamage;
            p.CriticalChance = stats.CriticalChance;
            p.CriticalDamage = stats.CriticalDamage;

            p.Strength = stats.Strength;
            p.Dexterity = stats.Dexterity;
        }

        return p;
    }
}