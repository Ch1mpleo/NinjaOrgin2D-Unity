using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using BayatGames.SaveGameFree;

/// <summary>
/// Authentication Manager - Qu?n lý logic và d? li?u xác th?c
/// T??ng t? nh? ReviveQuest - ch? qu?n lý data và logic, không qu?n lý UI
/// </summary>
[DefaultExecutionOrder(-100)]
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Header("Test Account (For Development)")]
    [SerializeField] private bool createTestAccount = true;
    [SerializeField] private string testUsername = "admin";
    [SerializeField] private string testPassword = "admin123";

    [Serializable]
    public class User
    {
        public string Username;
        public string PasswordHash; // base64
        public string Salt;         // base64
        public string CreatedAt;
        public string PlayerId;     // link to player's profile (GUID)
    }

    private List<User> users = new List<User>();
    public User CurrentUser { get; private set; }
    private const string USERS_KEY = "users";

    // Events: UIManager và AuthFlowManager s? subscribe vào các events này
    public event Action<User> OnUserLoggedIn;
    public event Action<User> OnUserLoggedOut;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadUsers();
            CreateTestAccountIfNeeded();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Test Account Setup

    /// <summary>
    /// T?o test account ?? test UI mà không c?n register
    /// </summary>
    private void CreateTestAccountIfNeeded()
    {
        if (!createTestAccount)
        {
            return;
        }

        // Ki?m tra xem test account ?ã t?n t?i ch?a
        if (UserExists(testUsername))
        {
            Debug.Log($"AuthManager: Test account '{testUsername}' already exists.");
            return;
        }

        // T?o test account
        Debug.Log($"AuthManager: Creating test account - Username: '{testUsername}', Password: '{testPassword}'");

        var salt = GenerateSalt();
        var hash = HashPassword(testPassword, salt);
        var playerId = "test-player-id"; // Fixed ID cho test account

        var testUser = new User
        {
            Username = testUsername,
            Salt = Convert.ToBase64String(salt),
            PasswordHash = Convert.ToBase64String(hash),
            CreatedAt = DateTime.UtcNow.ToString("o"),
            PlayerId = playerId
        };

        // T?o default profile cho test account
        var profile = PlayerProfile.CreateDefault();
        profile.Id = playerId;
        profile.Username = testUsername;
        SaveProfile(playerId, profile);

        // Save test user
        users.Add(testUser);
        SaveUsers();

        Debug.Log($"AuthManager: Test account created successfully!");
        Debug.Log($"AuthManager: You can login with - Username: '{testUsername}', Password: '{testPassword}'");
    }

    #endregion

    #region User Data Management

    private void LoadUsers()
    {
        // FIX L?I 2: Check if file exists tr??c khi load
        if (SaveGame.Exists(USERS_KEY))
        {
            var loaded = SaveGame.Load<List<User>>(USERS_KEY, new List<User>());
            users = loaded ?? new List<User>();
            Debug.Log($"AuthManager: Loaded {users.Count} users from save file.");
        }
        else
        {
            users = new List<User>();
            Debug.Log("AuthManager: No users file found. Starting with empty user list.");
        }
    }

    private void SaveUsers()
    {
        SaveGame.Save<List<User>>(USERS_KEY, users);
        Debug.Log($"AuthManager: Saved {users.Count} users to file.");
    }

    public bool UserExists(string username)
    {
        return users.Exists(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Authentication Logic

    /// <summary>
    /// ??ng ký user m?i
    /// T??ng t? nh? RevivePanel.OnSubmit() - x? lý logic và tr? v? k?t qu?
    /// </summary>
    public bool Register(string username, string password, out string error)
    {
        error = null;

        // Validation
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            error = "Username and password required.";
            return false;
        }

        if (UserExists(username))
        {
            error = "Username already taken.";
            return false;
        }

        // Create user
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);
        var playerId = Guid.NewGuid().ToString();

        var user = new User
        {
            Username = username,
            Salt = Convert.ToBase64String(salt),
            PasswordHash = Convert.ToBase64String(hash),
            CreatedAt = DateTime.UtcNow.ToString("o"),
            PlayerId = playerId
        };

        // Create default profile
        var profile = PlayerProfile.CreateDefault();
        profile.Id = playerId;
        profile.Username = username;
        SaveProfile(playerId, profile);

        // Save user
        users.Add(user);
        SaveUsers();

        Debug.Log($"AuthManager: User '{username}' registered successfully.");
        return true;
    }

    /// <summary>
    /// ??ng nh?p user
    /// T??ng t? nh? RevivePanel.OnSubmit() - x? lý logic và trigger event
    /// </summary>
    public bool Login(string username, string password, out string error)
    {
        error = null;

        // Find user
        var user = users.Find(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            error = $"User '{username}' not found.";
            Debug.LogWarning($"AuthManager: Login failed - {error}");
            return false;
        }

        // Verify password
        var salt = Convert.FromBase64String(user.Salt);
        var expected = Convert.FromBase64String(user.PasswordHash);
        var actual = HashPassword(password, salt);

        if (!AreEqual(expected, actual))
        {
            error = "Invalid password.";
            Debug.LogWarning($"AuthManager: Login failed - {error}");
            return false;
        }

        // Set current user and notify
        CurrentUser = user;
        OnUserLoggedIn?.Invoke(user);

        Debug.Log($"AuthManager: User '{username}' logged in successfully.");
        return true;
    }

    /// <summary>
    /// ??ng xu?t user
    /// T??ng t? nh? RevivePanel.OnSkip() - trigger callback
    /// </summary>
    public void Logout()
    {
        if (CurrentUser != null)
        {
            Debug.Log($"AuthManager: User '{CurrentUser.Username}' logged out.");
            
            // Notify listeners tr??c khi clear CurrentUser
            OnUserLoggedOut?.Invoke(CurrentUser);
            CurrentUser = null;
        }
    }

    #endregion

    #region Profile Management

    /// <summary>
    /// L?u profile c?a player
    /// T??ng t? nh? cách ReviveQuest qu?n lý questions
    /// </summary>
    public void SaveProfile(string playerId, PlayerProfile profile)
    {
        if (string.IsNullOrEmpty(playerId) || profile == null)
        {
            return;
        }

        SaveGame.Save($"player_{playerId}", profile);
        Debug.Log($"AuthManager: Saved profile for player '{playerId}'.");
    }

    /// <summary>
    /// Load profile c?a player
    /// </summary>
    public PlayerProfile LoadProfile(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        var profileKey = $"player_{playerId}";
        
        // Check if profile exists
        if (SaveGame.Exists(profileKey))
        {
            var profile = SaveGame.Load<PlayerProfile>(profileKey, PlayerProfile.CreateDefault());
            Debug.Log($"AuthManager: Loaded profile for player '{playerId}'.");
            return profile;
        }
        else
        {
            Debug.Log($"AuthManager: No profile found for player '{playerId}'. Creating default profile.");
            return PlayerProfile.CreateDefault();
        }
    }

    #endregion

    #region Security Utilities

    private static byte[] GenerateSalt(int size = 16)
    {
        var salt = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        var pwdBytes = Encoding.UTF8.GetBytes(password);
        var data = new byte[salt.Length + pwdBytes.Length];
        Buffer.BlockCopy(salt, 0, data, 0, salt.Length);
        Buffer.BlockCopy(pwdBytes, 0, data, salt.Length, pwdBytes.Length);

        using (var sha = SHA256.Create())
        {
            return sha.ComputeHash(data);
        }
    }

    private static bool AreEqual(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }

    #endregion
}