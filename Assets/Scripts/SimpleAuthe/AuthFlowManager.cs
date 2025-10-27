using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-50)]
public class AuthFlowManager : MonoBehaviour
{
    public static AuthFlowManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Optional: assign the login Canvas GameObject (contains AuthUI). If null the manager will try to find AuthUI in scene)")]
    public GameObject loginCanvas;

    [Header("Scene")]
    [Tooltip("Optional: scene name to load when user logs in. If empty the manager will try to apply profile to Player in current scene.")]
    public string mainSceneName;

    private PlayerProfile pendingProfile;

    // Cached references để tránh FindObjectOfType lặp lại
    private AuthManager subscribedAuthManager;
    private AuthUI cachedAuthUI;
    private Player cachedPlayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
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

        // show login UI at start
        ShowLoginUI(true);
    }

    private void OnDestroy()
    {
        if (subscribedAuthManager != null)
        {
            subscribedAuthManager.OnUserLoggedIn -= HandleUserLoggedIn;
            subscribedAuthManager.OnUserLoggedOut -= HandleUserLoggedOut;
            subscribedAuthManager = null;
        }
    }

    private void ShowLoginUI(bool show)
    {
        if (loginCanvas != null)
        {
            loginCanvas.SetActive(show);
            return;
        }

        // cache the AuthUI reference to avoid repeated Find calls
        if (cachedAuthUI == null)
        {
            cachedAuthUI = UnityEngine.Object.FindAnyObjectByType<AuthUI>();
        }

        if (cachedAuthUI != null)
        {
            cachedAuthUI.gameObject.SetActive(show);
            return;
        }

        // nothing to show/hide
        if (show)
        {
            Debug.LogWarning("ShowLoginUI: no loginCanvas assigned and no AuthUI found in scene.");
        }
    }

    private void HandleUserLoggedIn(AuthManager.User user)
    {
        ShowLoginUI(false);

        if (user == null)
        {
            Debug.LogWarning("HandleUserLoggedIn: user is null");
            return;
        }

        // prefer the live instance, but fall back to the subscribed manager
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
            // load target scene and apply profile after it's loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(mainSceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // clear cached player because scene changed
        cachedPlayer = null;
        // also clear cached AuthUI because UI may be part of the new scene
        cachedAuthUI = null;
        ApplyProfileToExistingPlayer();
    }

    private void ApplyProfileToExistingPlayer()
    {
        if (pendingProfile == null)
        {
            Debug.LogWarning("No profile loaded to apply to Player.");
            return;
        }

        // try cached player first, otherwise find and cache
        if (cachedPlayer == null)
        {
            cachedPlayer = UnityEngine.Object.FindAnyObjectByType<Player>();
        }

        if (cachedPlayer != null)
        {
            cachedPlayer.ApplyProfile(pendingProfile);
            pendingProfile = null;
        }
        else
        {
            Debug.LogWarning("Player object not found in scene. Make sure a Player exists or set mainSceneName to load the gameplay scene.");
        }
    }

    private void HandleUserLoggedOut(AuthManager.User user)
    {
        // when logging out, save current player profile (if any) using the user info provided
        if (user != null)
        {
            // try cached player first
            if (cachedPlayer == null)
            {
                cachedPlayer = UnityEngine.Object.FindAnyObjectByType<Player>();
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

        // show login UI
        ShowLoginUI(true);
    }
}
