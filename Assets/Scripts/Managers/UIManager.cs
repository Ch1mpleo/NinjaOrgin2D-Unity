using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats;

    [Header("Bars")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image manaBar;
    [SerializeField] private Image expBar;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI levelTMP;
    [SerializeField] private TextMeshProUGUI healthTMP;
    [SerializeField] private TextMeshProUGUI manaTMP;
    [SerializeField] private TextMeshProUGUI expTMP;
    [SerializeField] private TextMeshProUGUI coinsTMP;

    [Header("Stats Panel")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI statLevelTMP;
    [SerializeField] private TextMeshProUGUI statDamageTMP;
    [SerializeField] private TextMeshProUGUI statCChanceTMP;
    [SerializeField] private TextMeshProUGUI statCDamageTMP;
    [SerializeField] private TextMeshProUGUI statTotalExpTMP;
    [SerializeField] private TextMeshProUGUI statCurrentExpTMP;
    [SerializeField] private TextMeshProUGUI statRequiredExpTMP;
    [SerializeField] private TextMeshProUGUI attributePointsTMP;
    [SerializeField] private TextMeshProUGUI strengthTMP;
    [SerializeField] private TextMeshProUGUI dexterityTMP;
    [SerializeField] private TextMeshProUGUI intelligenceTMP;

    [Header("Extra Panels")]
    [SerializeField] private GameObject npcQuestPanel;
    [SerializeField] private GameObject playerQuestPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject craftingPanel;

    [Header("Revive System")]
    [SerializeField] private RevivePanel revivePanel;
    [Tooltip("Số lần thử trả lời câu hỏi revive")]
    [SerializeField] private int reviveAttempts = 3;

    private void Update()
    {
        UpdatePlayerUI();
    }

    public void OpenCloseStatsPanel()
    {
        statsPanel.SetActive(!statsPanel.activeSelf);
        if (statsPanel.activeSelf)
        {
            UpdateStatsPanel();
        }
    }

    public void OpenCloseNPCQuestPanel(bool value)
    {
        npcQuestPanel.SetActive(value);
    }

    public void OpenClosePlayerQuestPanel(bool value)
    {
        playerQuestPanel.SetActive(value);
    }

    public void OpenCloseShopPanel(bool value)
    {
        shopPanel.SetActive(value);
    }

    public void OpenCloseCraftPanel(bool value)
    {
        craftingPanel.SetActive(value);
    }

    /// <summary>
    /// Hiển thị Revive Panel với câu hỏi ngẫu nhiên
    /// Được gọi từ PlayerHealth khi người chơi chết
    /// </summary>
    public void ShowRevivePanel(PlayerHealth playerHealth)
    {
        if (revivePanel == null)
        {
            Debug.LogWarning("UIManager: RevivePanel chưa được gán trong Inspector!");
            return;
        }

        if (playerHealth == null)
        {
            Debug.LogError("UIManager: PlayerHealth không được null!");
            return;
        }

        // Lấy câu hỏi từ ReviveQuest singleton
        ReviveQuest reviveQuest = ReviveQuest.Instance;
        if (reviveQuest == null)
        {
            reviveQuest = UnityEngine.Object.FindAnyObjectByType<ReviveQuest>();
            if (reviveQuest == null)
            {
                Debug.LogError("UIManager: Không tìm thấy ReviveQuest trong scene!");
                return;
            }
        }

        // Lấy câu hỏi ngẫu nhiên từ ReviveQuest
        ReviveQuestion randomQuestion = reviveQuest.GetRandomQuestion();

        if (randomQuestion == null)
        {
            Debug.LogError("UIManager: Không thể lấy câu hỏi từ ReviveQuest!");
            return;
        }

        // Hiển thị panel với callback khi thất bại
        revivePanel.Show(randomQuestion, playerHealth, reviveAttempts, OnReviveFailed);
    }

    /// <summary>
    /// Callback khi người chơi thất bại trong việc revive
    /// </summary>
    private void OnReviveFailed()
    {
        Debug.Log("UIManager: Người chơi đã thất bại trong việc revive. Game Over!");
        // Có thể thêm logic: hiển thị Game Over screen, restart level, v.v.
    }

    /// <summary>
    /// Đóng Revive Panel (có thể gọi từ bên ngoài nếu cần)
    /// </summary>
    public void CloseRevivePanel()
    {
        if (revivePanel != null)
        {
            revivePanel.gameObject.SetActive(false);
        }
    }

    private void UpdatePlayerUI()
    {
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount,
            stats.Health / stats.MaxHealth, 10f * Time.deltaTime);
        manaBar.fillAmount = Mathf.Lerp(manaBar.fillAmount,
            stats.Mana / stats.MaxMana, 10f * Time.deltaTime);
        expBar.fillAmount = Mathf.Lerp(expBar.fillAmount,
            stats.CurrentExp / stats.NextLevelExp, 10f * Time.deltaTime);

        levelTMP.text = $"Level {stats.Level}";
        healthTMP.text = $"{stats.Health} / {stats.MaxHealth}";
        manaTMP.text = $"{stats.Mana} / {stats.MaxMana}";
        expTMP.text = $"{stats.CurrentExp} / {stats.NextLevelExp}";
        coinsTMP.text = CoinManager.Instance.Coins.ToString();
    }

    private void UpdateStatsPanel()
    {
        statLevelTMP.text = stats.Level.ToString();
        statDamageTMP.text = stats.TotalDamage.ToString();
        statCChanceTMP.text = stats.CriticalChance.ToString();
        statCDamageTMP.text = stats.CriticalDamage.ToString();
        statTotalExpTMP.text = stats.TotalExp.ToString();
        statCurrentExpTMP.text = stats.CurrentExp.ToString();
        statRequiredExpTMP.text = stats.NextLevelExp.ToString();

        attributePointsTMP.text = $"Points: {stats.AttributePoints}";
        strengthTMP.text = stats.Strength.ToString();
        dexterityTMP.text = stats.Dexterity.ToString();
        intelligenceTMP.text = stats.Intelligence.ToString();
    }

    private void UpgradeCallback()
    {
        UpdateStatsPanel();
    }

    private void ExtraInteractionCallback(InteractionType type)
    {
        switch (type)
        {
            case InteractionType.Quest:
                OpenCloseNPCQuestPanel(true);
                break;
            case InteractionType.Shop:
                OpenCloseShopPanel(true);
                break;
            case InteractionType.Crafting:
                OpenCloseCraftPanel(true);
                break;
        }
    }

    private void OnEnable()
    {
        PlayerUpgrade.OnPlayerUpgradeEvent += UpgradeCallback;
        DialogueManager.OnExtraInteractionEvent += ExtraInteractionCallback;
    }

    private void OnDisable()
    {
        PlayerUpgrade.OnPlayerUpgradeEvent -= UpgradeCallback;
        DialogueManager.OnExtraInteractionEvent -= ExtraInteractionCallback;
    }
}
