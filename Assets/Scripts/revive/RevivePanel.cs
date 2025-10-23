using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RevivePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionTMP;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button submitButton;

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
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        submitButton.onClick.AddListener(OnSubmit);
        gameObject.SetActive(false);
    }

    private void OnSubmit()
    {
        string userAnswer = (answerInput.text ?? string.Empty).Trim();
        string correct = (currentQuestion?.CorrectAnswer ?? string.Empty).Trim();

        if (string.Equals(userAnswer, correct, System.StringComparison.OrdinalIgnoreCase))
        {
            playerHealth.Revive();
            gameObject.SetActive(false);
            return;
        }

        // wrong answer
        attemptsLeft--;
        answerInput.text = "";

        if (attemptsLeft <= 0)
        {
            // No attempts left -> fail
            gameObject.SetActive(false);
            onFail?.Invoke();
        }
        else
        {
            // Optionally: show remaining attempts
            // e.g. update a UI label (not implemented)
        }
    }
}