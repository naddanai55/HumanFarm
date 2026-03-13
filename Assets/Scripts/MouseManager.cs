using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour
{
    public enum ToolType
    {
        Pointer,
        Food,
        Broom,
        Execute
    }

    [Header("Current Tool")]
    public ToolType currentTool = ToolType.Pointer;

    [Header("Food Tool Settings")]
    [SerializeField] float dropOffsetY = 1.0f;
    public GameObject itemPrefab;
    public LayerMask dropLayer;
    public LayerMask humanLayer;
    public LayerMask trashLayer;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Safety check: Are we clicking on a UI Button? If yes, stop here!
        // We don't want to drop food behind the UI menu.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Note: Using old Input system based on your code. 
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void HandleMouseClick()
    {
        // Create the ray laser from the mouse position once
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // 2. Decide what to do based on the current tool!
        switch (currentTool)
        {
            case ToolType.Pointer:
                HandlePointer(ray);
                break;
            case ToolType.Food:
                HandleFoodDrop(ray);
                break;
            case ToolType.Broom:
                HandleBroom(ray);
                break;
            case ToolType.Execute:
                HandleExecute(ray);
                break;
        }
    }

    // --- TOOL ACTIONS ---

    void HandleFoodDrop(Ray ray)
    {
        // This is your exact original code!
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, dropLayer))
        {
            Vector3 spawnPosition = hit.point;
            Instantiate(itemPrefab, spawnPosition + Vector3.up * dropOffsetY, Quaternion.identity);
        }
    }

    void HandlePointer(Ray ray)
    {
        // Example: Click a human to open their UI panel
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, humanLayer))
        {
            Debug.Log("Clicked on Human: " + hit.collider.gameObject.name);
            // OpenUI(hit.collider.GetComponent<HumanAI>());
        }
    }

    void HandleBroom(Ray ray)
    {
        // Example: Click trash to destroy it
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, trashLayer))
        {
            Debug.Log("Swept Trash!");
            Destroy(hit.collider.gameObject);
        }
    }

    void HandleExecute(Ray ray)
    {
        // Example: Click a fully grown human to harvest brain
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, humanLayer))
        {
            HumanAI human = hit.collider.GetComponent<HumanAI>();
            if (human != null && human.currentState == HumanAI.HumanState.ReadyToHarvest)
            {
                Debug.Log("Harvested Brain!");
                // Spawn Brain Item, Add Money, Destroy Human
            }
        }
    }

    // --- UI CAN CALL THIS TO CHANGE TOOLS ---

    public void SelectTool(int toolIndex)
    {
        // 0 = Pointer, 1 = Food, 2 = Broom, 3 = Execute
        currentTool = (ToolType)toolIndex;
        Debug.Log("Tool changed to: " + currentTool.ToString());
    }
}