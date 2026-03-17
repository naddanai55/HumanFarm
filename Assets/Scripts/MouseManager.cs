using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour
{
    // 1. Only TWO tools now!
    public enum ToolType
    {
        Feed,
        Kill
    }

    [Header("Current Tool")]
    public ToolType currentTool = ToolType.Feed;
    public bool toolActive = false; // Track if a tool is currently selected

    [Header("Prefabs & Effects")]
    [HideInInspector] public GameObject currentFoodPrefab; 
    [HideInInspector] public FoodSO currentFoodData;
    public float dropOffsetY = 1.0f;
    public GameObject sweepParticlePrefab; // Dust cloud for sweeping poop

    [Header("Layers")]
    public LayerMask dropLayer;         // Floor
    public LayerMask humanLayer;        // Humans
    public LayerMask trashLayer;        // Poop
    public LayerMask collectibleLayer;  // NEW: Brains!

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Don't click through UI buttons!
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void HandleMouseClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // We shoot ONE raycast that hits EVERYTHING. Then we figure out what we hit based on its Layer!
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            int hitLayer = hit.collider.gameObject.layer;

            // ==========================================
            // PRIORITY 1: UNIVERSAL ACTIONS (Always work!)
            // ==========================================

            // Did we click Poop? (Sweep it)
            if (((1 << hitLayer) & trashLayer) != 0)
            {
                HandleSweep(hit);
                return; // Stop checking! We did our action.
            }

            // Did we click a Brain? (Collect it)
            if (((1 << hitLayer) & collectibleLayer) != 0)
            {
                HandleCollectBrain(hit);
                return; // Stop checking!
            }

            // ==========================================
            // PRIORITY 2: EQUIPPED TOOL ACTIONS
            // ==========================================

            // Only execute tool actions if a tool is currently selected
            if (!toolActive)
                return;

            if (currentTool == ToolType.Feed)
            {
                // Feed Tool: Click Human -> Show Info
                if (((1 << hitLayer) & humanLayer) != 0)
                {
                    Debug.Log("Selected Human: " + hit.collider.gameObject.name);
                    // OpenUI(hit.collider.GetComponent<HumanAI>());
                }
                // Feed Tool: Click Floor -> Drop Food
                else if (((1 << hitLayer) & dropLayer) != 0)
                {
                    HandleFoodDrop(hit);
                }
            }
            else if (currentTool == ToolType.Kill)
            {
                // Kill Tool: Click Human -> Execute
                if (((1 << hitLayer) & humanLayer) != 0)
                {
                    HandleExecute(hit);
                }
                else
                {
                    Debug.Log("You missed! You can only kill humans.");
                }
            }
        }
    }

    // --- ACTION METHODS ---

    void HandleSweep(RaycastHit hit)
    {
        Debug.Log("Swept Poop!");
        if (sweepParticlePrefab != null)
        {
            Instantiate(sweepParticlePrefab, hit.transform.position, Quaternion.identity);
        }
        Destroy(hit.collider.gameObject);
    }

    void HandleCollectBrain(RaycastHit hit)
    {
        Debug.Log("Collected a Brain!");
        // TODO: Add to Inventory/GameManager here later!
        
        Destroy(hit.collider.gameObject);
    }

    void HandleFoodDrop(RaycastHit hit)
    {
        if (currentFoodPrefab == null) return;

        if (InventoryManager.Instance.UseFood(currentFoodPrefab))
        {
            Vector3 spawnPosition = hit.point;
            Instantiate(currentFoodPrefab, spawnPosition + Vector3.up * dropOffsetY, Quaternion.identity);

            // ---> REFRESH THE BUTTONS SO THE NUMBER GOES DOWN! <---
            FindAnyObjectByType<UIToolbar>().RefreshToolbar();    
        }
        else
        {
            Debug.Log("Out of Food!");
        }
    }

    void HandleExecute(RaycastHit hit)
    {
        HumanAI human = hit.collider.GetComponent<HumanAI>();
        if (human != null)
        {
            human.ExecuteHuman(); // Kills them immediately!
        }
    }

    // --- UI CAN CALL THIS TO CHANGE TOOLS ---
    public void SelectTool(int toolIndex)
    {
        // 0 = Feed, 1 = Kill
        currentTool = (ToolType)toolIndex;
        toolActive = true;
        Debug.Log("Tool changed to: " + currentTool.ToString());
    }

    // --- UI CALLS THIS WHEN TOGGLING OFF A BUTTON ---
    public void DeactivateTool()
    {
        toolActive = false;
        Debug.Log("Tool deactivated!");
    }
}