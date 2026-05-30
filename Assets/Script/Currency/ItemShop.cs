using UnityEngine;

public class ItemShop : MonoBehaviour
{
    [Header("Panel Toko")]
    public GameObject itemShopPanel;  // Tarik objek UI kontener Item Shop di sini
    public GameObject statsShopPanel;
    
    [Header("Harga Potion")]
    public int smallPotionPrice = 10;
    public int mediumPotionPrice = 90;
    public int largePotionPrice = 900;
    public int smallMPPotionPrice = 30;
    public int energyPotionPrice = 50;
    public int strengthPotionPrice = 75;
    public int speedPotionPrice = 60;

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

    public void BuyMediumPotion()
    {
        if (PlayerStats.instance.gold >= mediumPotionPrice)
        {
            PlayerStats.instance.gold -= mediumPotionPrice;
            PlayerStats.instance.mediumPotionCount++;
            Debug.Log("Membeli Medium Potion. Sisa Gold: " + PlayerStats.instance.gold);
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

    public void BuysmallMPPotion()
    {
        if (PlayerStats.instance.gold >= smallMPPotionPrice)
        {
            PlayerStats.instance.gold -= smallMPPotionPrice;
            PlayerStats.instance.smallMPCount++;
            Debug.Log("Membeli MP Potion.");
        }
    }

    public void BuyEnergyPotion()
    {
        if (PlayerStats.instance.gold >= energyPotionPrice)
        {
            PlayerStats.instance.gold -= energyPotionPrice;
            PlayerStats.instance.energyPotionCount++;
            Debug.Log("Membeli Energy Potion.");
        }
    }

    public void BuyStrengthPotion()
    {
        if (PlayerStats.instance.gold >= strengthPotionPrice)
        {
            PlayerStats.instance.gold -= strengthPotionPrice;
            PlayerStats.instance.strengthPotionCount++;
            Debug.Log("Membeli Strength Potion.");
        }
    }

    public void BuySpeedPotion()
    {
        if (PlayerStats.instance.gold >= speedPotionPrice)
        {
            PlayerStats.instance.gold -= speedPotionPrice;
            PlayerStats.instance.speedPotionCount++;
            Debug.Log("Membeli Speed Potion.");
        }
    }
}