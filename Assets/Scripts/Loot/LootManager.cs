using UnityEngine;

public class LootManager : Singleton<LootManager>
{
    [Header("Config")]
    [SerializeField] private GameObject lootPanel;
    [SerializeField] private LootButton lootButtonPrefab;
    [SerializeField] private Transform container;

    public void ShowLoot(EnemyLoot enemyLoot)
    {
        lootPanel.SetActive(true);
        if (LootPanelWithItems())
        {
            for (int i = 0; i < container.childCount; i++)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }

        foreach (DropItem item in enemyLoot.Items)
        {
            if (item.PickedItem) continue;
            LootButton lootButton = Instantiate(lootButtonPrefab, container);
            lootButton.ConfigLootButton(item);
        }
    }

    public void ClosePanel()
    {
        lootPanel.SetActive(false);
    }
    
    private bool LootPanelWithItems()
    {
        return container.childCount > 0;
    }
}