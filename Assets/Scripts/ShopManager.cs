using UnityEngine;

public class ShopManager : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private UIToolbar toolbar;
    void Start()
    {
        gameManager = GetComponent<GameManager>();
    }

    public void BuyHuman(HumanAI humanPrefab)
    {
        if (humanPrefab == null || humanPrefab.humanData == null)
        {
            Debug.LogError("Tried to buy a null human or missing HumanData!");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("ShopManager is missing a GameManager reference on the same GameObject.");
            return;
        }

        int cost = humanPrefab.humanData.purchaseCost;

        if (gameManager.SpendZCoins(cost))
        {
            Instantiate(humanPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log($"Bought {humanPrefab.humanData.HumanName} for {cost} Z-Coins!");
            
        }
        else
        {
            Debug.Log("Not enough Z-Coins to buy this human!");
        }
    }

    public void BuyFood(FoodSO foodData)
    {
        if (foodData == null || foodData.foodPrefab == null)
        {
            Debug.LogError("Tried to buy a null FoodSO or missing Food prefab!");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("ShopManager is missing a GameManager reference on the same GameObject.");
            return;
        }

        int cost = foodData.purchaseCost;

        if (gameManager.SpendZCoins(cost))
        {
            Debug.Log($"Bought {foodData.foodName} for {cost} Z-Coins!");

            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager.Instance is missing in the scene.");
                return;
            }

            InventoryManager.Instance.AddFood(foodData.foodPrefab, 1);
            toolbar?.RefreshToolbar();
        }
        else
        {
            Debug.Log("Not enough Z-Coins to buy this food!");
        }
    }
}