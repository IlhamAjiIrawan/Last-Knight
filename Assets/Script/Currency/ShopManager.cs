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
    // --- LOGIKA UPGRADE STATS DENGAN RUMUS EKSPONENSIAL ---
    public void UpgradeHealth()
    {
        if (PlayerStats.instance.gold >= PlayerStats.instance.healthUpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.healthUpgradeCost;
            PlayerStats.instance.maxHealth += 10;
            PlayerStats.instance.currentHealth += 10; 
            
            // PERBAIKAN: Naikkan level upgrade health
            PlayerStats.instance.healthLevel++;
            UpdateUI();
        }
    }

    public void UpgradeMP()
    {
        if (PlayerStats.instance.gold >= PlayerStats.instance.mpUpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.mpUpgradeCost;
            PlayerStats.instance.maxMP += 10;
            PlayerStats.instance.mpUpgradeCount++;

            if (PlayerStats.instance.mpUpgradeCount % 3 == 0)
            {
                PlayerStats.instance.mpRegenRate += 1;
                Debug.Log("Upgrade ke-" + PlayerStats.instance.mpUpgradeCount + "! MP Regen Rate bertambah +1. Sekarang: " + PlayerStats.instance.mpRegenRate);
            }
            else
            {
                Debug.Log("Upgrade MP Berhasil. But " + (3 - (PlayerStats.instance.mpUpgradeCount % 3)) + " upgrade lagi untuk menambah MP Regen.");
            }

            // PERBAIKAN: Naikkan level upgrade MP
            PlayerStats.instance.mpLevel++;
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

            // PERBAIKAN: Naikkan level upgrade Energy
            PlayerStats.instance.energyLevel++;
            UpdateUI();
        }
    }

    public void UpgradeDamage()
    {
        if (PlayerStats.instance.gold >= PlayerStats.instance.damageUpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.damageUpgradeCost;
            PlayerStats.instance.damage += 1;

            // PERBAIKAN: Naikkan level upgrade Damage
            PlayerStats.instance.damageLevel++;
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

            // PERBAIKAN: Naikkan level upgrade Speed
            PlayerStats.instance.speedLevel++;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        // Tetap membaca nama variabel yang sama karena di PlayerStats.cs properti rumusnya sengaja disamakan hurufnya
        if (healthPriceText) healthPriceText.text = " " + PlayerStats.instance.healthUpgradeCost;
        if (mpPriceText) mpPriceText.text = " " + PlayerStats.instance.mpUpgradeCost;
        if (energyPriceText) energyPriceText.text = " " + PlayerStats.instance.energyUpgradeCost;
        if (damagePriceText) damagePriceText.text = " " + PlayerStats.instance.damageUpgradeCost;
        if (speedPriceText) speedPriceText.text = " " + PlayerStats.instance.speedUpgradeCost;
    }

    public void CloseShop()
    {
        FindObjectOfType<WaveManager>().GoToNextWave();
    }
}