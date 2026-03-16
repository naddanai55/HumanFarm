using UnityEngine;

public class EmojiBillboard : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] float xOffset = 10f;
    void Start()
    {
        mainCamera = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera != null)
        {
            Vector3 rot = mainCamera.transform.rotation.eulerAngles;
            rot.x += xOffset;
            Vector3 pos = transform.position;
            pos.z -= 2f;
            transform.rotation = Quaternion.Euler(rot);
            transform.position = pos;
        }
    }
}
