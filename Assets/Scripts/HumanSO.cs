using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "HumanSO", menuName = "Scriptable Objects/HumanSO")]
public class HumanSO : ScriptableObject
{
    [Header("Identity")]
    public string HumanName;
    public Sprite profilePic; // (Optional: If you want profile pictures in the UI!)
    public float maxHunger = 100f;
    public float maxHappiness = 100f;
    public float maxGrowth = 100f;
    public float bowelCapacity = 100f;

    [Header("AI Thresholds")]
    public float seekFoodThreshold = 50f;
    public float starveThreshold = 0f;

    [Header("Rates (Per Second)")]
    public float hungerDepletionRate = 2f;
    public float happinessDepletionRate = 1f;
    public float happinessDepletionOverTime = 0.5f;
    public float bowelRate = 0.5f;

    [Tooltip("The speed the brain grows when Happiness is at 100%")]
    public float maxBrainGrowthRate = 5f;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Economy")]
    public int purchaseCost = 50;

    [Tooltip("Base value (in Z-Coins) when Happiness is 100%.")]
    public int baseValueZCoins = 100;
}