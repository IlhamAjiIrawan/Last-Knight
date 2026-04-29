using UnityEngine;
using TMPro; // Jika kamu menggunakan TextMeshPro untuk menampilkan harga

public class ShopManager : MonoBehaviour
{
    // Hubungkan teks harga di Inspector jika ingin menampilkan harga yang berubah
    public TextMeshProUGUI healthPriceText;
    public TextMeshProUGUI mpPriceText;
    public TextMeshProUGUI energyPriceText;
    public TextMeshProUGUI damagePriceText;
    public TextMeshProUGUI speedPriceText;

    private void OnEnable()
    {
        UpdateUI(); // Update teks harga saat shop dibuka
    }

    public void UpgradeHealth()
    {
        if (PlayerStats.instance.gold >= PlayerStats.instance.healthUpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.healthUpgradeCost;
            
            // Efek Upgrade
            PlayerStats.instance.maxHealth += 10;
            PlayerStats.instance.currentHealth += 10; // Bonus: isi nyawa saat upgrade
            
            // Lipat gandakan biaya
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
            
            // Update speed langsung ke PlayerMovement jika sedang aktif
            PlayerMovement pm = FindObjectOfType<PlayerMovement>();
            if (pm != null) pm.speed = PlayerStats.instance.speed;

            PlayerStats.instance.speedUpgradeCost *= 2;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        // Update teks harga di UI (jika variabel teks diisi di Inspector)
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
