using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HumanInfoUI : MonoBehaviour
{
    public static HumanInfoUI Instance;

    [Header("UI Elements")]
    public Image profileImage;
    public TextMeshProUGUI nameText;
    public Slider hungerBar;
    public Slider happinessBar;
    public Slider growthBar;
    // public Slider poopBar;
    public Button closeButton;

    [Header("Slide Animation")]
    public float slideSpeed = 10f;
    private RectTransform rectTransform;
    private float hiddenPosX; // Where it hides (off-screen)
    private float visiblePosX = 0f; // Where it rests (on-screen)
    private Coroutine slideCoroutine;

    // The human we are currently looking at
    private HumanAI selectedHuman;

    void Awake()
    {
        if (Instance == null) Instance = this;
        rectTransform = GetComponent<RectTransform>();
        
        // Calculate the hidden position based on the panel's width
        hiddenPosX = rectTransform.rect.width; 
        
        // Make sure it starts hidden
        rectTransform.anchoredPosition = new Vector2(hiddenPosX, rectTransform.anchoredPosition.y);

        // Hook up the close button
        closeButton.onClick.AddListener(HidePanel);
    }

    void Update()
    {
        // If the panel is open and a human is selected, update the bars live!
        if (selectedHuman != null)
        {
            hungerBar.value = selectedHuman.currentHunger;
            happinessBar.value = selectedHuman.currentHappiness;
            growthBar.value = selectedHuman.currentGrowth;
            // poopBar.value = selectedHuman.currentBowelLevel;
        }
    }

    public void ShowPanel(HumanAI human)
    {
        selectedHuman = human;

        // Set the static info from the HumanSO
        nameText.text = human.humanData.HumanName;
        // (Optional: If you add 'public Sprite profilePic;' to HumanSO!)
        profileImage.sprite = human.humanData.profilePic;

        // Set the max values for the sliders
        hungerBar.maxValue = human.humanData.maxHunger;
        happinessBar.maxValue = human.humanData.maxHappiness;
        growthBar.maxValue = human.humanData.maxGrowth;
        // poopBar.maxValue = human.humanData.bowelCapacity;

        // Start the slide-in animation!
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideTo(visiblePosX));
    }

    public void HidePanel()
    {
        selectedHuman = null;

        // Start the slide-out animation!
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideTo(hiddenPosX));
    }

    // A simple, smooth sliding animation
    private IEnumerator SlideTo(float targetX)
    {
        float currentX = rectTransform.anchoredPosition.x;
        
        while (Mathf.Abs(currentX - targetX) > 0.5f)
        {
            currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * slideSpeed);
            rectTransform.anchoredPosition = new Vector2(currentX, rectTransform.anchoredPosition.y);
            yield return null;
        }

        // Snap to the exact final position at the end
        rectTransform.anchoredPosition = new Vector2(targetX, rectTransform.anchoredPosition.y);
    }
}