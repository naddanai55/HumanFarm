using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour
{
    public enum ToolType
    {
        Feed,
        Kill,
        None
    }

    [Header("Current Tool")]
    public ToolType currentTool = ToolType.None;
    public bool toolActive = false;

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


    private HumanInfoUI humanInfoUI;
    private GameManager gameManager;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        gameManager = FindFirstObjectByType<GameManager>();
        humanInfoUI = FindFirstObjectByType<HumanInfoUI>();
    }

    void Update()
    {
        // Don't click through UI buttons!
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            HandleMouseClick();
        }
        if (Input.GetMouseButtonDown(1)) // Right-click to deselect tool
        {
            DeactivateTool();
        }
    }

    void HandleMouseClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            int hitLayer = hit.collider.gameObject.layer;

            if (((1 << hitLayer) & trashLayer) != 0)
            {
                HandleSweep(hit);
                return;
            }

            if (((1 << hitLayer) & collectibleLayer) != 0)
            {
                HandleCollectBrain(hit);
                return;
            }

            if (((1 << hitLayer) & humanLayer) != 0)
            {
                HandleShowInfo(hit);
            }

            if (!toolActive)
                return;

            if (currentTool == ToolType.Feed)
            {
                if (((1 << hitLayer) & dropLayer) != 0)
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
        BrainItem brain = hit.collider.GetComponent<BrainItem>();
        int value = brain != null ? brain.zCoinValue : 0;

        if (value > 0 && gameManager != null)
        {
            gameManager.AddZCoins(value);
        }

        Debug.Log($"Collected a Brain! (+{value} Z-Coins)");
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

    void HandleShowInfo(RaycastHit hit)
    {
        HumanAI human = hit.collider.GetComponent<HumanAI>();
        if (human != null)
        {
            humanInfoUI.ShowPanel(human);
            
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
        currentTool = ToolType.None;
        toolActive = false;
        Debug.Log("Tool deactivated!");
    }
}