using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using BayatGames.SaveGameFree;

[DefaultExecutionOrder(-100)]
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Serializable]
    public class User
    {
        public string Username;
        public string PasswordHash; // base64
        public string Salt;         // base64
        public string CreatedAt;
        // link to player's profile (GUID)
        public string PlayerId;
    }

    private List<User> users = new List<User>();
    public User CurrentUser { get; private set; }
    private const string USERS_KEY = "users";

    // Events: subscribe to react when user logs in/out
    public event Action<User> OnUserLoggedIn;
    public event Action<User> OnUserLoggedOut;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
        LoadUsers();
    }

    void LoadUsers()
    {
        var loaded = SaveGame.Load<List<User>>(USERS_KEY, new List<User>());
        users = loaded ?? new List<User>();
    }

    void SaveUsers()
    {
        SaveGame.Save<List<User>>(USERS_KEY, users);
    }

    public bool UserExists(string username)
    {
        return users.Exists(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    public bool Register(string username, string password, out string error)
    {
        error = null;
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
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);
        var playerId = Guid.NewGuid().ToString();
        var u = new User
        {
            Username = username,
            Salt = Convert.ToBase64String(salt),
            PasswordHash = Convert.ToBase64String(hash),
            CreatedAt = DateTime.UtcNow.ToString("o"),
            PlayerId = playerId
        };

        // create default profile for the new player
        var profile = PlayerProfile.CreateDefault();
        profile.Id = playerId;
        profile.Username = username;
        SaveProfile(playerId, profile);

        users.Add(u);
        SaveUsers();
        return true;
    }

    public bool Login(string username, string password, out string error)
    {
        error = null;
        var user = users.Find(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            error = "User not found.";
            return false;
        }
        var salt = Convert.FromBase64String(user.Salt);
        var expected = Convert.FromBase64String(user.PasswordHash);
        var actual = HashPassword(password, salt);
        if (!AreEqual(expected, actual))
        {
            error = "Invalid credentials.";
            return false;
        }
        CurrentUser = user;
        // notify listeners
        OnUserLoggedIn?.Invoke(user);
        return true;
    }

    public void Logout()
    {
        if (CurrentUser != null)
        {
            // notify listeners before clearing CurrentUser so they can save state if needed
            OnUserLoggedOut?.Invoke(CurrentUser);
        }
        CurrentUser = null;
    }

    // Profile persistence helpers
    public void SaveProfile(string playerId, PlayerProfile profile)
    {
        if (string.IsNullOrEmpty(playerId) || profile == null) return;
        SaveGame.Save($"player_{playerId}", profile);
    }

    public PlayerProfile LoadProfile(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return null;
        return SaveGame.Load<PlayerProfile>($"player_{playerId}", default(PlayerProfile));
    }

    // Utility: generate 16-byte salt
    private static byte[] GenerateSalt(int size = 16)
    {
        var salt = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    // Utility: SHA256(salt + password)
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
        if (a == null || b == null || a.Length != b.Length) return false;
        var result = 0;
        for (int i = 0; i < a.Length; i++) result |= a[i] ^ b[i];
        return result == 0;
    }
}