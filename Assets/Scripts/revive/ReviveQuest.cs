using UnityEngine;

/// <summary>
/// Data class đại diện cho một câu hỏi revive
/// </summary>
[System.Serializable]
public class ReviveQuestion
{
    [TextArea(2, 4)]
    public string QuestionText;
    public string CorrectAnswer;
}

/// <summary>
/// Quản lý danh sách câu hỏi revive
/// Gắn component này vào GameObject trong scene để cấu hình câu hỏi qua Inspector
/// </summary>
public class ReviveQuest : MonoBehaviour
{
    public static ReviveQuest Instance { get; private set; }

    [Header("Revive Questions Configuration")]
    [Tooltip("Danh sách các câu hỏi revive. Sẽ random một câu khi người chơi chết.")]
    [SerializeField]
    private ReviveQuestion[] questions = new ReviveQuestion[]
    {
        new ReviveQuestion { QuestionText = "2 + 2 bằng mấy?", CorrectAnswer = "4" },
        new ReviveQuestion { QuestionText = "Thủ đô của Việt Nam là gì?", CorrectAnswer = "Hà Nội" },
        new ReviveQuestion { QuestionText = "Màu của lá cây là gì?", CorrectAnswer = "Xanh" }
    };

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate questions
        if (questions == null || questions.Length == 0)
        {
            Debug.LogWarning("ReviveQuest: Không có câu hỏi nào được thiết lập. Thêm câu hỏi trong Inspector!");
        }
    }

    /// <summary>
    /// Lấy một câu hỏi ngẫu nhiên từ danh sách
    /// </summary>
    public ReviveQuestion GetRandomQuestion()
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogError("ReviveQuest: Không có câu hỏi nào! Trả về câu hỏi mặc định.");
            return new ReviveQuestion
            {
                QuestionText = "Câu hỏi mặc định: 1 + 1 = ?",
                CorrectAnswer = "2"
            };
        }

        int randomIndex = Random.Range(0, questions.Length);
        return questions[randomIndex];
    }

    /// <summary>
    /// Lấy câu hỏi theo index cụ thể (để test hoặc chọn câu hỏi theo level)
    /// </summary>
    public ReviveQuestion GetQuestionByIndex(int index)
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogError("ReviveQuest: Không có câu hỏi nào!");
            return null;
        }

        if (index < 0 || index >= questions.Length)
        {
            Debug.LogWarning($"ReviveQuest: Index {index} nằm ngoài phạm vi. Trả về câu hỏi đầu tiên.");
            return questions[0];
        }

        return questions[index];
    }

    /// <summary>
    /// Thêm câu hỏi mới vào danh sách (runtime)
    /// </summary>
    public void AddQuestion(string questionText, string correctAnswer)
    {
        ReviveQuestion newQuestion = new ReviveQuestion
        {
            QuestionText = questionText,
            CorrectAnswer = correctAnswer
        };

        // Expand array
        ReviveQuestion[] newArray = new ReviveQuestion[questions.Length + 1];
        for (int i = 0; i < questions.Length; i++)
        {
            newArray[i] = questions[i];
        }
        newArray[questions.Length] = newQuestion;
        questions = newArray;

        Debug.Log($"ReviveQuest: Đã thêm câu hỏi mới. Tổng số câu hỏi: {questions.Length}");
    }

    /// <summary>
    /// Lấy tổng số câu hỏi hiện có
    /// </summary>
    public int GetQuestionCount()
    {
        return questions != null ? questions.Length : 0;
    }
}