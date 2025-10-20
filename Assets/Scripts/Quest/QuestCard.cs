using TMPro;
using UnityEngine;

public class QuestCard : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private TextMeshProUGUI questNameTMP;
    [SerializeField] private TextMeshProUGUI questDescriptionTMP;

    public Quest QuestToComplete { get; set; }

    public virtual void ConfigQuestUI(Quest quest)
    {
        QuestToComplete = quest;
        questNameTMP.text = quest.Name;
        questDescriptionTMP.text = quest.Description;
    }
}
