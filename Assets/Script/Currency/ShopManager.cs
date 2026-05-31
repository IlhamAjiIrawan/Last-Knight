using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Panel Toko (Pusat Navigasi)")]
    public GameObject itemShopPanel;  
    public GameObject statsShopPanel;
    public GameObject skillShopPanel; // Menghubungkan panel skill baru
    
    [Header("UI Teks Harga Stats")]
    public TextMeshProUGUI healthPriceText;
    public TextMeshProUGUI mpPriceText;
    public TextMeshProUGUI energyPriceText;
    public TextMeshProUGUI damagePriceText;
    public TextMeshProUGUI speedPriceText;

    private void OnEnable()
    {
        UpdateUI(); // Update teks harga saat shop dibuka
    }

    // --- FUNGSI NAVIGASI ANTI-TUMPANG TINDIH ---
    public void SwitchToItemShop()
    {
        if (itemShopPanel != null && statsShopPanel != null && skillShopPanel != null)
        {
            itemShopPanel.SetActive(true);
            statsShopPanel.SetActive(false);
            skillShopPanel.SetActive(false); // Matikan panel lain
            Debug.Log("Berpindah ke Item Shop.");
        }
    }

    public void SwitchToStatsShop()
    {
        if (itemShopPanel != null && statsShopPanel != null && skillShopPanel != null)
        {
            itemShopPanel.SetActive(false);
            statsShopPanel.SetActive(true);
            skillShopPanel.SetActive(false); // Matikan panel lain
            Debug.Log("Berpindah ke Stats Shop.");
        }
    }

    public void SwitchToSkillShop()
    {
        if (itemShopPanel != null && statsShopPanel != null && skillShopPanel != null)
        {
            itemShopPanel.SetActive(false);
            statsShopPanel.SetActive(false);
            skillShopPanel.SetActive(true);  // Aktifkan panel skill
            Debug.Log("Berpindah ke Skill Shop.");
        }
    }
    
    // --- LOGIKA UPGRADE STATS ---
    public void UpgradeHealth()
    {
        if (PlayerStats.instance.gold >= PlayerStats.instance.healthUpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.healthUpgradeCost;
            PlayerStats.instance.maxHealth += 10;
            PlayerStats.instance.currentHealth += 10; 
            PlayerStats.instance.healthUpgradeCost *= 2;
            UpdateUI();
        }
    }

    public void UpgradeMP()
    {
        if (PlayerStats.instance.gold >= PlayerStats.instance.mpUpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.mpUpgradeCost;
            PlayerStats.instance.maxMP += 10;
            PlayerStats.instance.mpRegenRate += 1;
            PlayerStats.instance.mpUpgradeCost *= 2;
            UpdateUI();
        }
    }

    public void UpgradeEnergy()
    {
        if (PlayerStats.instance.gold >= PlayerStats.instance.energyUpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.energyUpgradeCost;
            PlayerStats.instance.maxEnergy += 5;
            PlayerStats.instance.energyRegenRate += 1;
            PlayerStats.instance.energyUpgradeCost *= 2;
            UpdateUI();
        }
    }

    public void UpgradeDamage()
    {
        if (PlayerStats.instance.gold >= PlayerStats.instance.damageUpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.damageUpgradeCost;
            PlayerStats.instance.damage += 1;
            PlayerStats.instance.damageUpgradeCost *= 2;
            UpdateUI();
        }
    }

    public void UpgradeSpeed()
    {
        if (PlayerStats.instance.gold >= PlayerStats.instance.speedUpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.speedUpgradeCost;
            PlayerStats.instance.speed += 1;
            
            PlayerMovement pm = FindObjectOfType<PlayerMovement>();
            if (pm != null) pm.speed = PlayerStats.instance.speed;

            PlayerStats.instance.speedUpgradeCost *= 2;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (healthPriceText) healthPriceText.text = "Cost: " + PlayerStats.instance.healthUpgradeCost;
        if (mpPriceText) mpPriceText.text = "Cost: " + PlayerStats.instance.mpUpgradeCost;
        if (energyPriceText) energyPriceText.text = "Cost: " + PlayerStats.instance.energyUpgradeCost;
        if (damagePriceText) damagePriceText.text = "Cost: " + PlayerStats.instance.damageUpgradeCost;
        if (speedPriceText) speedPriceText.text = "Cost: " + PlayerStats.instance.speedUpgradeCost;
    }

    public void CloseShop()
    {
        FindObjectOfType<WaveManager>().GoToNextWave();
    }
}