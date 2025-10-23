using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI to test AuthManager (local register/login/logout).
/// Attach this component to a Canvas GameObject and wire the UI references.
/// </summary>
public class AuthUI : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;

    public Button registerButton;
    public Button loginButton;
    public Button logoutButton;

    public Text messageText;
    public Text currentUserText;

    void Start()
    {
        if (registerButton != null) registerButton.onClick.AddListener(OnRegisterClicked);
        if (loginButton != null) loginButton.onClick.AddListener(OnLoginClicked);
        if (logoutButton != null) logoutButton.onClick.AddListener(OnLogoutClicked);
        UpdateCurrentUser();
    }

    void OnDestroy()
    {
        if (registerButton != null) registerButton.onClick.RemoveListener(OnRegisterClicked);
        if (loginButton != null) loginButton.onClick.RemoveListener(OnLoginClicked);
        if (logoutButton != null) logoutButton.onClick.RemoveListener(OnLogoutClicked);
    }

    public void OnRegisterClicked()
    {
        ClearMessage();
        if (!ValidateInputs(out string username, out string password)) return;
        if (AuthManager.Instance == null)
        {
            ShowMessage("AuthManager instance not found.");
            return;
        }
        if (AuthManager.Instance.Register(username, password, out string error))
        {
            ShowMessage("Registration successful.");
            UpdateCurrentUser();
            ClearFields();
        }
        else
        {
            ShowMessage("Register failed: " + error);
        }
    }

    public void OnLoginClicked()
    {
        ClearMessage();
        if (!ValidateInputs(out string username, out string password)) return;
        if (AuthManager.Instance == null)
        {
            ShowMessage("AuthManager instance not found.");
            return;
        }
        if (AuthManager.Instance.Login(username, password, out string error))
        {
            ShowMessage("Login successful.");
            UpdateCurrentUser();
            ClearFields();
        }
        else
        {
            ShowMessage("Login failed: " + error);
        }
    }

    public void OnLogoutClicked()
    {
        ClearMessage();
        if (AuthManager.Instance == null)
        {
            ShowMessage("AuthManager instance not found.");
            return;
        }
        AuthManager.Instance.Logout();
        ShowMessage("Logged out.");
        UpdateCurrentUser();
    }

    bool ValidateInputs(out string username, out string password)
    {
        username = usernameInput != null ? usernameInput.text.Trim() : string.Empty;
        password = passwordInput != null ? passwordInput.text : string.Empty;
        if (string.IsNullOrEmpty(username))
        {
            ShowMessage("Username is required.");
            return false;
        }
        if (string.IsNullOrEmpty(password))
        {
            ShowMessage("Password is required.");
            return false;
        }
        return true;
    }

    void UpdateCurrentUser()
    {
        if (currentUserText == null) return;
        if (AuthManager.Instance != null && AuthManager.Instance.CurrentUser != null)
        {
            currentUserText.text = "Current: " + AuthManager.Instance.CurrentUser.Username;
        }
        else
        {
            currentUserText.text = "Not logged in";
        }
    }

    void ShowMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
        else Debug.Log(msg);
    }

    void ClearMessage()
    {
        if (messageText != null) messageText.text = string.Empty;
    }

    void ClearFields()
    {
        if (usernameInput != null) usernameInput.text = string.Empty;
        if (passwordInput != null) passwordInput.text = string.Empty;
    }
}
