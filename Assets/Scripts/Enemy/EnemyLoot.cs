using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyLoot : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float expDrop;
    [SerializeField] private DropItem[] dropItems;

    public List<DropItem> Items { get; private set; }
    public float ExpDrop => expDrop;

    private void Start()
    {
        LoadDropItems();
    }

    private void LoadDropItems()
    {
        Items = new List<DropItem>();
        foreach (DropItem item in dropItems)
        {
            float prob = Random.Range(0f, 100f);
            if (prob <= item.DropChance)
            {
                Items.Add(item);
            }
        }
    }
}

[Serializable]
public class DropItem
{
    [Header("Config")]
    public string Name;
    public InventoryItem Item;
    public int Quantity;

    [Header("Drop Chance")]
    public float DropChance;
    public bool PickedItem { get; set; }
}
