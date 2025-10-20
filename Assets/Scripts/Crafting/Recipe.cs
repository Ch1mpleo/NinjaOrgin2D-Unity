using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Recipe
{
    public string Name;
    [Header("Item 1")] 
    public InventoryItem Item1;
    public int Item1Amount;
    [Header("Item 2")] 
    public InventoryItem Item2;
    public int Item2Amount;
    [Header("Final Item")] 
    public InventoryItem FinalItem;
    public int FinalItemAmount;
}
