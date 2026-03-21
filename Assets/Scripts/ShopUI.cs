using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject humanTab;
    [SerializeField] GameObject foodTab;

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        ShowHumanTab();
    }
    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    public void ShowHumanTab()
    {
        humanTab.SetActive(true);
        foodTab.SetActive(false);
    }

    public void ShowFoodTab()
    {
        humanTab.SetActive(false);
        foodTab.SetActive(true);
    }

}