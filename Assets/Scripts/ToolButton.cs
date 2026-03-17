using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToolButton : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TextMeshProUGUI amountText;

    private bool selected = false;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color selectedColor = new Color(1f, 0.84f, 0f, 1f); // Gold/Yellow
    
    public void SetSelected(bool isSelected)
    {
        selected = isSelected;
        
        if (selected)
        {
            button.image.color = selectedColor;
        }
        else
        {
            button.image.color = normalColor;
        }
    }
}
