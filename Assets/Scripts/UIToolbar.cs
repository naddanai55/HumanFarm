using UnityEngine;
using UnityEngine.UI;

public class UIToolbar : MonoBehaviour
{
    public MouseManager mouseManager;
    
    [Header("UI Setup")]
    public GameObject toolButtonPrefab; // Drag your Button Prefab here
    public Transform buttonContainer;   // Drag your ToolbarContainer here

    [Header("Knife Settings")]
    public Sprite knifeIcon;            // Drag your Knife picture here

    private ToolButton selectedButton;  // Track the currently selected button

    void Start()
    {
        RefreshToolbar();
        // Auto-select the Knife button (always first)
        if (buttonContainer.childCount > 0)
        {
            SelectButton(buttonContainer.GetChild(0).GetComponent<ToolButton>());
        }
    }

    // Call this whenever you Buy food or Drop food!
    public void RefreshToolbar()
    {
        // 1. Delete all old buttons in the container
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        selectedButton = null; // Reset selection since we're destroying buttons

        // 2. Always create the Knife Button first
        CreateKnifeButton();

        // 3. Loop through your inventory and create a button for each food!
        foreach (var slot in InventoryManager.Instance.foodInventory)
        {
            // Only show the button if we actually own some of this food
            if (slot.amount > 0) 
            {
                CreateFoodButton(slot);
            }
        }
    }

    void CreateKnifeButton()
    {
        // Spawn the button inside the container
        GameObject newBtnObj = Instantiate(toolButtonPrefab, buttonContainer);
        ToolButton btnScript = newBtnObj.GetComponent<ToolButton>();

        // Set Icon and Text
        btnScript.icon.sprite = knifeIcon;
        btnScript.amountText.text = "KILL";

        // Tell the button what to do when clicked!
        btnScript.button.onClick.AddListener(() =>
        {
            SelectButton(btnScript);
            mouseManager.currentTool = MouseManager.ToolType.Kill;
            mouseManager.currentFoodPrefab = null;
            Debug.Log("Equipped Knife!");
        });
    }

    void CreateFoodButton(FoodInventorySlot slot)
    {
        GameObject newBtnObj = Instantiate(toolButtonPrefab, buttonContainer);
        ToolButton btnScript = newBtnObj.GetComponent<ToolButton>();

        // Get the FoodSO from the prefab to find its name and picture
        FoodSO data = slot.foodPrefab.GetComponent<FoodItem>().foodData;

        // Set Icon and Text (e.g., "x5")
        btnScript.icon.sprite = data.foodIcon;
        btnScript.amountText.text = "x" + slot.amount;

        // Tell the button what to do when clicked!
        btnScript.button.onClick.AddListener(() =>
        {
            SelectButton(btnScript);
            mouseManager.currentTool = MouseManager.ToolType.Feed;
            mouseManager.currentFoodPrefab = slot.foodPrefab;
            Debug.Log("Equipped " + data.foodName);
        });
    }

    /// <summary>
    /// Selects a button and deselects the previously selected one.
    /// Clicking the same button again toggles it OFF.
    /// </summary>
    public void SelectButton(ToolButton newButton)
    {
        // If clicking the same button, toggle it off
        if (selectedButton == newButton)
        {
            selectedButton.SetSelected(false);
            selectedButton = null;
            mouseManager.DeactivateTool(); // Disable the tool when toggling off
            return;
        }
        
        // Deselect the previous button
        if (selectedButton != null)
        {
            selectedButton.SetSelected(false);
        }

        // Select the new button
        selectedButton = newButton;
        selectedButton.SetSelected(true);
        mouseManager.toolActive = true; // Enable tool when selecting
    }
}