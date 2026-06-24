using UnityEngine;
using TMPro;

public class SkillShop : MonoBehaviour
{
    [Header("Panel Toko")]
    public GameObject itemShopPanel;  
    public GameObject statsShopPanel;
    public GameObject skillShopPanel;
    
    [Header("UI Teks Harga Toko")]
    public TextMeshProUGUI skill1PriceText;
    public TextMeshProUGUI skill2PriceText;

    void Update()
    {
        // Menampilkan info harga upgrade skill secara realtime
        if (skill1PriceText != null)
        {
            skill1PriceText.text = PlayerStats.instance.skill1Level >= PlayerStats.instance.maxSkillLevel ? 
                "MAX" : "Cost: " + PlayerStats.instance.skill1UpgradeCost + " G";
        }
        if (skill2PriceText != null)
        {
            skill2PriceText.text = PlayerStats.instance.skill2Level >= PlayerStats.instance.maxSkillLevel ? 
                "MAX" : "Cost: " + PlayerStats.instance.skill2UpgradeCost + " G";
        }
    }

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

    public void UpgradeSkill1()
    {
        if (PlayerStats.instance.skill1Level < PlayerStats.instance.maxSkillLevel && 
            PlayerStats.instance.gold >= PlayerStats.instance.skill1UpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.skill1UpgradeCost;
            PlayerStats.instance.skill1Level++;
            
            PlayerStats.instance.skill1UpgradeCost *= 2;
            
            Debug.Log("Skill 1 berhasil di-unlock/upgrade! Level: " + PlayerStats.instance.skill1Level);
        }
    }

   public void UpgradeSkill2()
    {
        if (PlayerStats.instance.skill2Level < PlayerStats.instance.maxSkillLevel && 
            PlayerStats.instance.gold >= PlayerStats.instance.skill2UpgradeCost)
        {
            // 1. Kurangi koin player sesuai harga saat ini
            PlayerStats.instance.gold -= PlayerStats.instance.skill2UpgradeCost;
            
            // 2. Naikkan level skill
            PlayerStats.instance.skill2Level++;

            // 3. Hitung harga untuk UPGRADE BERIKUTNYA otomatis
            PlayerStats.instance.skill2UpgradeCost = 125 * Mathf.RoundToInt(Mathf.Pow(2, PlayerStats.instance.skill2Level));

            Debug.Log("Skill 2 (Shield) Berhasil Di-upgrade! Level Saat Ini: " + PlayerStats.instance.skill2Level);
        }
    }
}