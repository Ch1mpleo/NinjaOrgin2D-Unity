using TMPro;
using UnityEngine;

public class QuestCardNPC : QuestCard
{
    [SerializeField] private TextMeshProUGUI questRewardTMP;

    public override void ConfigQuestUI(Quest quest)
    {
        base.ConfigQuestUI(quest);
        questRewardTMP.text = $"- {quest.GoldReward} Gold\n" +
                              $"- {quest.ExpReward} Exp\n" +
                              $"- x{quest.ItemReward.Quantity}{quest.ItemReward.Item.Name}";
    }

    public void AcceptQuest()
    {
        if (QuestToComplete == null) return;
        QuestToComplete.QuestAccepted = true;
        QuestManager.Instance.AcceptQuest(QuestToComplete);
        gameObject.SetActive(false);
    }
}