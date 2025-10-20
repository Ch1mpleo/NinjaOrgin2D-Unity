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

    public void Show(ReviveQuestion question, PlayerHealth player)
    {
        currentQuestion = question;
        playerHealth = player;
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
        if (answerInput.text.Trim().Equals(currentQuestion.CorrectAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
            playerHealth.Revive();
            gameObject.SetActive(false);
        }
        else
        {
            answerInput.text = "";
            // Optionally: Hiển thị thông báo sai
        }
    }
}