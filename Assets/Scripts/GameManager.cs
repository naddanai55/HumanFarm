using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int zCoins = 0;
    [SerializeField] TMP_Text zCoinText;

    void Update()
    {
        if (zCoinText != null)
        {
            zCoinText.text = $"Z-Coins: {zCoins}";
        }
    }

    public void AddZCoins(int amount)
    {
        if (amount <= 0) return;

        zCoins += amount;
        zCoins = Mathf.Max(zCoins, 0);
    }

    public bool SpendZCoins(int amount)
    {
        if (amount <= 0) return true;
        if (zCoins < amount) return false;

        zCoins -= amount;
        zCoins = Mathf.Max(zCoins, 0);
        return true;
    }

    public void SetZCoins(int newAmount)
    {
        zCoins = Mathf.Max(newAmount, 0);
    }
}
