using System;

[Serializable]
public class PlayerProfile
{
    public string Id;
    public string Username;

    public int Level;

    public float Health;
    public float MaxHealth;

    public float Mana;
    public float MaxMana;

    public float CurrentExp;
    public float NextLevelExp;
    public float InitialNextLevelExp;
    public float ExpMultiplier;

    public float BaseDamage;
    public float CriticalChance;
    public float CriticalDamage;

    public int Strength;
    public int Dexterity;

    public static PlayerProfile CreateDefault()
    {
        return new PlayerProfile
        {
            Level = 1,
            MaxHealth = 100f,
            Health = 100f,
            MaxMana = 50f,
            Mana = 50f,
            CurrentExp = 0f,
            NextLevelExp = 100f,
            InitialNextLevelExp = 100f,
            ExpMultiplier = 1.1f,
            BaseDamage = 5f,
            CriticalChance = 5f,
            CriticalDamage = 150f,
            Strength = 1,
            Dexterity = 1
        };
    }
}
