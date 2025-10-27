using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Authentication Panel - Qu?n lý UI elements và user interactions
/// T??ng t? nh? RevivePanel - ch? qu?n lý UI, không ch?a business logic
/// </summary>
public class AuthPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button loginButton;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI messageText;

    private System.Action<string, string> onRegisterCallback;
    private System.Action<string, string> onLoginCallback;

    /// <summary>
    /// Hi?n th? panel v?i callbacks
    /// T??ng t? nh? RevivePanel.Show()
    /// </summary>
    public void Show(
        System.Action<string, string> onRegister,
        System.Action<string, string> onLogin)
    {
        onRegisterCallback = onRegister;
        onLoginCallback = onLogin;

        ClearFields();
        ClearMessage();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// ?n panel
    /// T??ng t? nh? RevivePanel khi thành công
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        SetupButtons();
    }

    private void OnDestroy()
    {
        CleanupButtons();
    }

    #region Button Setup

    private void SetupButtons()
    {
        if (registerButton != null)
        {
            registerButton.onClick.AddListener(OnRegisterClicked);
        }

        if (loginButton != null)
        {
            loginButton.onClick.AddListener(OnLoginClicked);
        }
    }

    private void CleanupButtons()
    {
        if (registerButton != null)
        {
            registerButton.onClick.RemoveListener(OnRegisterClicked);
        }

        if (loginButton != null)
        {
            loginButton.onClick.RemoveListener(OnLoginClicked);
        }
    }

    #endregion

    #region Button Handlers

    /// <summary>
    /// X? lý khi click nút Register
    /// T??ng t? nh? RevivePanel.OnSubmit() - validate và invoke callback
    /// </summary>
    private void OnRegisterClicked()
    {
        ClearMessage();

        if (!ValidateInputs(out string username, out string password))
        {
            return;
        }

        // Invoke callback gi?ng nh? RevivePanel
        onRegisterCallback?.Invoke(username, password);
    }

    /// <summary>
    /// X? lý khi click nút Login
    /// T??ng t? nh? RevivePanel.OnSubmit()
    /// </summary>
    private void OnLoginClicked()
    {
        ClearMessage();

        if (!ValidateInputs(out string username, out string password))
        {
            return;
        }

        // Invoke callback gi?ng nh? RevivePanel
        onLoginCallback?.Invoke(username, password);
    }

    #endregion

    #region Validation & UI Updates

    /// <summary>
    /// Validate input fields
    /// T??ng t? nh? RevivePanel validate answer
    /// </summary>
    private bool ValidateInputs(out string username, out string password)
    {
        username = usernameInput != null ? usernameInput.text.Trim() : string.Empty;
        password = passwordInput != null ? passwordInput.text : string.Empty;

        if (string.IsNullOrEmpty(username))
        {
            ShowMessage("Username is required.", true);
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowMessage("Password is required.", true);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Hi?n th? message cho user v?i color coding
    /// T??ng t? nh? RevivePanel.UpdateAttemptsText()
    /// </summary>
    public void ShowMessage(string msg, bool isError = false)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            
            // ??i màu d?a trên lo?i message
            if (isError)
            {
                messageText.color = Color.red;
            }
            else
            {
                messageText.color = Color.green;
            }
        }
        else
        {
            Debug.Log(msg);
        }
    }

    private void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = string.Empty;
        }
    }

    private void ClearFields()
    {
        if (usernameInput != null)
        {
            usernameInput.text = string.Empty;
        }

        if (passwordInput != null)
        {
            passwordInput.text = string.Empty;
        }
    }

    #endregion
}
