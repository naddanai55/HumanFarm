using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [SerializeField] float dropOffsetY = 1.0f;
    public GameObject itemPrefab;
    public LayerMask dropLayer;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DropItem();
        }
    }

    void DropItem()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, dropLayer))
        {
            Vector3 spawnPosition = hit.point;

            Instantiate(itemPrefab, spawnPosition + Vector3.up * dropOffsetY, Quaternion.identity);
        }
    }
}