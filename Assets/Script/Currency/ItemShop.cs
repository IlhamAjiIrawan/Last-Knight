using UnityEngine;

public class ItemShop : MonoBehaviour
{
    [Header("Panel Toko")]
    public GameObject itemShopPanel;  // Tarik objek UI kontener Item Shop di sini
    public GameObject statsShopPanel;
    
    [Header("Harga Potion")]
    public int smallPotionPrice = 25;  // Harga bisa bervariasi
    public int largePotionPrice = 60;

    public void SwitchToStatsShop()
    {
        if (itemShopPanel != null && statsShopPanel != null)
        {
            itemShopPanel.SetActive(false);  // Sembunyikan Item Shop
            statsShopPanel.SetActive(true);  // Munculkan Stats Shop
            Debug.Log("Berpindah ke Stats Shop.");
        }
    }

    public void SwitchToItemShop()
    {
        if (itemShopPanel != null && statsShopPanel != null)
        {
            itemShopPanel.SetActive(true);   // Munculkan Item Shop
            statsShopPanel.SetActive(false);  // Sembunyikan Stats Shop
            Debug.Log("Berpindah ke Item Shop.");
        }
    }
    
    public void BuySmallPotion()
    {
        if (PlayerStats.instance.gold >= smallPotionPrice)
        {
            PlayerStats.instance.gold -= smallPotionPrice;
            PlayerStats.instance.smallPotionCount++;
            Debug.Log("Membeli Small Potion. Sisa Gold: " + PlayerStats.instance.gold);
        }
        else
        {
            Debug.Log("Gold tidak cukup untuk Small Potion!");
        }
    }

    public void BuyLargePotion()
    {
        if (PlayerStats.instance.gold >= largePotionPrice)
        {
            PlayerStats.instance.gold -= largePotionPrice;
            PlayerStats.instance.largePotionCount++;
            Debug.Log("Membeli Large Potion. Sisa Gold: " + PlayerStats.instance.gold);
        }
        else
        {
            Debug.Log("Gold tidak cukup untuk Large Potion!");
        }
    }
}