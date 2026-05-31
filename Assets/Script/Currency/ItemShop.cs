using UnityEngine;

public class ItemShop : MonoBehaviour
{
    [Header("Panel Toko")]
    public GameObject itemShopPanel;  
    public GameObject statsShopPanel;
    public GameObject skillShopPanel;
    
    [Header("Harga Potion")]
    public int smallPotionPrice = 10;
    public int mediumPotionPrice = 90;
    public int largePotionPrice = 900;
    public int smallMPPotionPrice = 30;
    public int energyPotionPrice = 50;
    public int strengthPotionPrice = 75;
    public int speedPotionPrice = 60;

    // Perbaikan Navigasi Mandiri di SkillShop.cs
    public void SwitchToStatsShop()
    {
        if (itemShopPanel != null && statsShopPanel != null && skillShopPanel != null)
        {
            itemShopPanel.SetActive(false);
            statsShopPanel.SetActive(true);
            skillShopPanel.SetActive(false); // Pastikan skill shop milik sendiri mati
            Debug.Log("Berpindah ke Stats Shop.");
        }
    }

    public void SwitchToItemShop()
    {
        if (itemShopPanel != null && statsShopPanel != null && skillShopPanel != null)
        {
            itemShopPanel.SetActive(true);
            statsShopPanel.SetActive(false);
            skillShopPanel.SetActive(false); // Pastikan skill shop milik sendiri mati
            Debug.Log("Berpindah ke Item Shop.");
        }
    }

    public void SwitchToSkillShop()
    {
        if (itemShopPanel != null && statsShopPanel != null && skillShopPanel != null)
        {
            itemShopPanel.SetActive(false);
            statsShopPanel.SetActive(false);
            skillShopPanel.SetActive(true);  // Aktifkan panel skill shop
            Debug.Log("Berpindah ke Skill Shop.");
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
    }

    public void BuyMediumPotion()
    {
        if (PlayerStats.instance.gold >= mediumPotionPrice)
        {
            PlayerStats.instance.gold -= mediumPotionPrice;
            PlayerStats.instance.mediumPotionCount++;
            Debug.Log("Membeli Medium Potion. Sisa Gold: " + PlayerStats.instance.gold);
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
    }

    public void BuysmallMPPotion()
    {
        if (PlayerStats.instance.gold >= smallMPPotionPrice)
        {
            PlayerStats.instance.gold -= smallMPPotionPrice;
            PlayerStats.instance.smallMPCount++; // Sinkron dengan variabel PlayerStats
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