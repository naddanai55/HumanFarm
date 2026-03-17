using UnityEngine;

public class BrainItem : MonoBehaviour
{
    [Header("Brain Data")]
    public string brainName = "Standard Brain";
    public int zCoinValue = 0; // The MouseManager will read this!

    // The HumanAI script will call this the exact moment the brain spawns
    public void SetupBrain(string name, int value)
    {
        brainName = name;
        zCoinValue = value;
    }
}