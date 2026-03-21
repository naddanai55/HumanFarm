using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemImage;
    public TMP_Text itemNameText;
    public TMP_Text costText;

    [SerializeField] private HumanSO humanData;
    [SerializeField] private FoodSO foodData;
    [SerializeField] bool isHuman = true;

    void Start()
    {
        if (isHuman && humanData != null)
        {
            itemImage.sprite = humanData.profilePic;
            itemNameText.text = humanData.HumanName;
            costText.text = humanData.purchaseCost.ToString();
        }
        else if (!isHuman && foodData != null)
        {
            itemImage.sprite = foodData.foodIcon;
            itemNameText.text = foodData.foodName;
            costText.text = foodData.purchaseCost.ToString();
        }
        
    }
}