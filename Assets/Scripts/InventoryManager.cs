using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FoodInventorySlot
{
    public GameObject foodPrefab; // ONLY the prefab!
    public int amount;            // How many we have left
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Player Inventory")]
    public List<FoodInventorySlot> foodInventory = new List<FoodInventorySlot>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Call this when you drop food
    public bool UseFood(GameObject prefabToUse)
    {
        foreach (var slot in foodInventory)
        {
            if (slot.foodPrefab == prefabToUse && slot.amount > 0)
            {
                slot.amount--; // Reduce quantity
                return true;   // Success!
            }
        }
        return false; // Out of food!
    }

    // Call this when you buy food from the shop.
    public void AddFood(GameObject foodPrefab, int amount = 1)
    {
        if (foodPrefab == null) return;
        if (amount <= 0) return;

        foreach (var slot in foodInventory)
        {
            if (slot.foodPrefab == foodPrefab)
            {
                slot.amount += amount;
                return;
            }
        }

        foodInventory.Add(new FoodInventorySlot
        {
            foodPrefab = foodPrefab,
            amount = amount
        });
    }
}