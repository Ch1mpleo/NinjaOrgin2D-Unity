using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-50)]
public class AuthFlowManager : MonoBehaviour
{
    public static AuthFlowManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Authentication Panel component")]
    [SerializeField] private AuthPanel authPanel;

    [Header("Player")]
    [Tooltip("Assign Player GameObject here. If empty, will try to find automatically.")]
    [SerializeField] private Player playerPrefabOrReference;

    [Header("Scene")]
    [Tooltip("Optional: scene name to load when user logs in. If empty the manager will try to apply profile to Player in current scene.")]
    public string mainSceneName;

    private PlayerProfile pendingProfile;

    // Cached references
    private AuthManager subscribedAuthManager;
    private Player cachedPlayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupAuthManager();
        
        // Ẩn Player lúc start nếu chưa login
        HidePlayerIfNotLoggedIn();
        
        ShowLoginPanel();
    }

    private void OnDestroy()
    {
        CleanupAuthManager();
    }

    #region Auth Manager Setup

    private void SetupAuthManager()
    {
        // subscribe to AuthManager events (AuthManager is DefaultExecutionOrder -100 so it should be ready)
        // prefer the singleton instance, fall back to finding any AuthManager in scene
        var am = AuthManager.Instance ?? UnityEngine.Object.FindAnyObjectByType<AuthManager>();
        if (am != null)
        {
            am.OnUserLoggedIn += HandleUserLoggedIn;
            am.OnUserLoggedOut += HandleUserLoggedOut;
            subscribedAuthManager = am;
        }
        else
        {
            Debug.LogWarning("AuthFlowManager: AuthManager not found. Auth events will not be handled until an AuthManager appears.");
        }
    }

    private void CleanupAuthManager()
    {
        if (subscribedAuthManager != null)
        {
            subscribedAuthManager.OnUserLoggedIn -= HandleUserLoggedIn;
            subscribedAuthManager.OnUserLoggedOut -= HandleUserLoggedOut;
            subscribedAuthManager = null;
        }
    }

    #endregion

    #region Auth Panel Management

    private void ShowLoginPanel()
    {
        if (authPanel == null)
        {
            authPanel = UnityEngine.Object.FindAnyObjectByType<AuthPanel>();
        }

        if (authPanel != null)
        {
            authPanel.Show(OnRegister, OnLogin);
        }
        else
        {
            Debug.LogWarning("AuthFlowManager: AuthPanel not found in scene.");
        }
    }

    private void HideLoginPanel()
    {
        if (authPanel != null)
        {
            authPanel.Hide();
        }
    }

    #endregion

    #region Auth Panel Callbacks

    private void OnRegister(string username, string password)
    {
        var mgr = AuthManager.Instance ?? subscribedAuthManager;
        if (mgr == null)
        {
            if (authPanel != null) authPanel.ShowMessage("AuthManager not available.");
            return;
        }

        if (mgr.Register(username, password, out string error))
        {
            if (authPanel != null)
            {
                authPanel.ShowMessage("Registration successful. Please login.");
            }
        }
        else
        {
            if (authPanel != null) authPanel.ShowMessage("Register failed: " + error);
        }
    }

    private void OnLogin(string username, string password)
    {
        var mgr = AuthManager.Instance ?? subscribedAuthManager;
        if (mgr == null)
        {
            if (authPanel != null) authPanel.ShowMessage("AuthManager not available.");
            return;
        }

        if (mgr.Login(username, password, out string error))
        {
            if (authPanel != null)
            {
                authPanel.ShowMessage("Login successful. Loading game...");
            }
            // HandleUserLoggedIn will be called by AuthManager event
        }
        else
        {
            if (authPanel != null) authPanel.ShowMessage("Login failed: " + error);
        }
    }

    #endregion

    #region User Login/Logout Handlers

    private void HandleUserLoggedIn(AuthManager.User user)
    {
        HideLoginPanel();

        if (user == null)
        {
            Debug.LogWarning("HandleUserLoggedIn: user is null");
            return;
        }

        var mgr = AuthManager.Instance ?? subscribedAuthManager;
        if (mgr == null)
        {
            Debug.LogWarning("HandleUserLoggedIn: no AuthManager available to load profile.");
            return;
        }

        pendingProfile = mgr.LoadProfile(user.PlayerId);

        if (string.IsNullOrEmpty(mainSceneName))
        {
            ApplyProfileToExistingPlayer();
        }
        else
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(mainSceneName);
        }
    }

    private void HandleUserLoggedOut(AuthManager.User user)
    {
        if (user != null)
        {
            SaveCurrentPlayerProfile(user);
        }

        ShowLoginPanel();
        if (authPanel != null)
        {
            authPanel.ShowMessage("Logged out successfully.");
        }
    }

    #endregion

    #region Scene Management

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ClearSceneCache();
        ApplyProfileToExistingPlayer();
    }

    private void ClearSceneCache()
    {
        // clear cached player because scene changed
        cachedPlayer = null;
        // Re-find authPanel in new scene if needed
        authPanel = UnityEngine.Object.FindAnyObjectByType<AuthPanel>();
    }

    #endregion

    #region Profile Management

    private void ApplyProfileToExistingPlayer()
    {
        if (pendingProfile == null)
        {
            Debug.LogWarning("AuthFlowManager: No profile loaded to apply to Player.");
            return;
        }

        // Priority 1: Use assigned player from Inspector
        if (cachedPlayer == null && playerPrefabOrReference != null)
        {
            cachedPlayer = playerPrefabOrReference;
            Debug.Log("AuthFlowManager: Using Player assigned in Inspector.");
        }

        // Priority 2: Find active Player in scene
        if (cachedPlayer == null)
        {
            cachedPlayer = UnityEngine.Object.FindAnyObjectByType<Player>(FindObjectsInactive.Exclude);
            if (cachedPlayer != null)
            {
                Debug.Log("AuthFlowManager: Found active Player in scene.");
            }
        }

        // Priority 3: Find inactive Player in scene (including disabled)
        if (cachedPlayer == null)
        {
            cachedPlayer = UnityEngine.Object.FindAnyObjectByType<Player>(FindObjectsInactive.Include);
            if (cachedPlayer != null)
            {
                Debug.Log("AuthFlowManager: Found inactive Player in scene.");
            }
        }

        if (cachedPlayer != null)
        {
            // Enable Player GameObject if disabled
            if (!cachedPlayer.gameObject.activeSelf)
            {
                cachedPlayer.gameObject.SetActive(true);
                Debug.Log("AuthFlowManager: Enabled Player GameObject after login.");
            }

            cachedPlayer.ApplyProfile(pendingProfile);
            pendingProfile = null;
            
            Debug.Log("AuthFlowManager: Applied profile to Player successfully.");
        }
        else
        {
            Debug.LogError("AuthFlowManager: Player object not found in scene! " +
                          "Please assign Player in AuthFlowManager Inspector or make sure a Player GameObject exists with Player.cs component.");
        }
    }

    private void SaveCurrentPlayerProfile(AuthManager.User user)
    {
        // try cached player first
        if (cachedPlayer == null)
        {
            cachedPlayer = playerPrefabOrReference;
        }

        if (cachedPlayer == null)
        {
            cachedPlayer = UnityEngine.Object.FindAnyObjectByType<Player>(FindObjectsInactive.Include);
        }

        if (cachedPlayer != null)
        {
            var profile = cachedPlayer.ToProfile();
            // ensure profile id is set to the user's player id
            profile.Id = user.PlayerId;

            var mgr = AuthManager.Instance ?? subscribedAuthManager;
            if (mgr != null)
            {
                mgr.SaveProfile(user.PlayerId, profile);
            }
            else
            {
                Debug.LogWarning("HandleUserLoggedOut: no AuthManager available to save profile.");
            }
        }
    }

    #endregion

    #region Player Management

    /// <summary>
    /// Ẩn Player nếu chưa có user login
    /// </summary>
    private void HidePlayerIfNotLoggedIn()
    {
        var authMgr = AuthManager.Instance ?? subscribedAuthManager;
        
        // Nếu chưa có user login, ẩn Player
        if (authMgr == null || authMgr.CurrentUser == null)
        {
            var player = playerPrefabOrReference;
            if (player == null)
            {
                player = UnityEngine.Object.FindAnyObjectByType<Player>(FindObjectsInactive.Exclude);
            }

            if (player != null && player.gameObject.activeSelf)
            {
                player.gameObject.SetActive(false);
                Debug.Log("AuthFlowManager: Hid Player GameObject until login.");
            }
        }
    }

    #endregion
}
