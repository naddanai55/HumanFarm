using UnityEngine;

public class BrainItem : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;

    [Header("Brain Data")]
    public string brainName = "Standard Brain";
    public int zCoinValue = 0;

    public void SetupBrain(string name, int value)
    {
        brainName = name;
        zCoinValue = value;
    }

    void Update()
    {
        transform.Rotate(-1 * Vector3.up, rotationSpeed * Time.deltaTime);
    }
}