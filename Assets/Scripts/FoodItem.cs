using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;
    public FoodSO foodData;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime); // Simple rotation for visual flair
    }

    public float GetEatingTime()
    {
        return foodData.eatingDuration;
    }

    // 2. Human calls this function when the timer finishes!
    public void Consume(HumanAI human)
    {
        // Add stats to the human
        human.currentHunger += foodData.nutritionValue;

        // Optional: Add happiness if your food has it
        human.currentHappiness += foodData.bonusHappiness;

        // Clamp so it doesn't go over max (Reads max from the human's SO)
        human.currentHunger = Mathf.Clamp(human.currentHunger, 0f, human.humanData.maxHunger);
        human.currentHappiness = Mathf.Clamp(human.currentHappiness, 0f, human.humanData.maxHappiness);

        // The food destroys ITSELF!
        Destroy(gameObject);
    }
}