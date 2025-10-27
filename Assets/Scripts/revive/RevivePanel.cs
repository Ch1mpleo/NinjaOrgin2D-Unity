using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RevivePanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI questionTMP;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button skipButton;

    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI attemptsText;

    private ReviveQuestion currentQuestion;
    private PlayerHealth playerHealth;
    private int attemptsLeft;
    private System.Action onFail;

    public void Show(ReviveQuestion question, PlayerHealth player, int attempts, System.Action onFailCallback)
    {
        currentQuestion = question;
        playerHealth = player;
        attemptsLeft = Mathf.Max(1, attempts);
        onFail = onFailCallback;
        questionTMP.text = question.QuestionText;
        answerInput.text = "";

        UpdateAttemptsText();
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmit);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkip);
        }

    }

    private void OnDestroy()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(OnSubmit);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(OnSkip);
        }
    }

    private void OnSubmit()
    {
        string userAnswer = (answerInput.text ?? string.Empty).Trim();
        string correct = (currentQuestion?.CorrectAnswer ?? string.Empty).Trim();

        if (string.Equals(userAnswer, correct, System.StringComparison.OrdinalIgnoreCase))
        {
            // Đáp án đúng
            playerHealth.Revive();
            gameObject.SetActive(false);
            return;
        }

        // Đáp án sai
        attemptsLeft--;
        answerInput.text = "";
        UpdateAttemptsText();

        if (attemptsLeft <= 0)
        {
            // Hết lượt thử
            gameObject.SetActive(false);
            onFail?.Invoke();
        }
    }

    private void OnSkip()
    {
        Debug.Log("RevivePanel: Người chơi đã bỏ qua revive.");
        gameObject.SetActive(false);
        onFail?.Invoke();
    }

    private void UpdateAttemptsText()
    {
        if (attemptsText != null)
        {
            attemptsText.text = $"Còn {attemptsLeft} lần thử";

            // Đổi màu khi còn ít lượt
            if (attemptsLeft == 1)
            {
                attemptsText.color = Color.red;
            }
            else if (attemptsLeft == 2)
            {
                attemptsText.color = new Color(1f, 0.5f, 0f); // Orange
            }
            else
            {
                attemptsText.color = Color.green;
            }
        }
    }
}