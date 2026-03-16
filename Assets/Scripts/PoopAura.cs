using UnityEngine;

public class PoopAura : MonoBehaviour
{
    public float stinkDamagePerSecond = 1f;
    private void OnTriggerStay(Collider other)
    {
        // 1. Did a human walk into the stink zone?
        HumanAI human = other.GetComponent<HumanAI>();

        if (human != null)
        {
            // 2. Drain their happiness quickly!
            human.currentHappiness -= stinkDamagePerSecond * Time.deltaTime;

            // 3. Keep it clamped so it doesn't drop below 0
            human.currentHappiness = Mathf.Clamp(human.currentHappiness, 0f, human.humanData.maxHappiness);
        }
    }
}
