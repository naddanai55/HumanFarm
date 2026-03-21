using UnityEngine;

[CreateAssetMenu(fileName = "FoodSO", menuName = "Scriptable Objects/FoodSO")]
public class FoodSO : ScriptableObject
{
    [Header("Identity")]
    public string foodName;
    public Sprite foodIcon;
    public GameObject foodPrefab;

    [Header("Eating Mechanics")]
    public float nutritionValue = 30f;
    public float eatingDuration = 2f;
    public float bonusHappiness = 10f;
    public float wasteValue = 50f;
    public int purchaseCost = 20;
}