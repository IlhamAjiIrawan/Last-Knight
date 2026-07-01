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
    public TextMeshProUGUI skill3PriceText;
    public TextMeshProUGUI skill4PriceText;

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
        if (skill3PriceText != null)
        {
            skill3PriceText.text = PlayerStats.instance.skill3Level >= PlayerStats.instance.maxSkillLevel ? 
                "MAX" : "Cost: " + PlayerStats.instance.skill3UpgradeCost + " G";
        }
        if (skill4PriceText != null)
        {
            skill4PriceText.text = PlayerStats.instance.skill4Level >= PlayerStats.instance.maxSkillLevel ? 
                "MAX" : "Cost: " + PlayerStats.instance.skill4UpgradeCost + " G";
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

    public void UpgradeSkill3()
    {
        if (PlayerStats.instance.skill3Level < PlayerStats.instance.maxSkillLevel && 
            PlayerStats.instance.gold >= PlayerStats.instance.skill3UpgradeCost)
        {
            // 1. Kurangi koin player sesuai harga saat ini
            PlayerStats.instance.gold -= PlayerStats.instance.skill3UpgradeCost;
            
            // 2. Naikkan level skill 3
            PlayerStats.instance.skill3Level++;

            // 3. Hitung harga untuk UPGRADE BERIKUTNYA secara otomatis (Pola kelipatan 2)
            // Lvl 0->1 = 250 | Lvl 1->2 = 500 | Lvl 2->3 = 1000 | Lvl 3->4 = 2000 | Lvl 4->5 = 4000
            PlayerStats.instance.skill3UpgradeCost = 250 * Mathf.RoundToInt(Mathf.Pow(2, PlayerStats.instance.skill3Level));

            Debug.Log("Skill 3 (Horizontal Slash) Berhasil Di-upgrade! Level Saat Ini: " + PlayerStats.instance.skill3Level);
        }
        else
        {
            Debug.Log("Gagal Upgrade Skill 3: Gold tidak cukup atau Level sudah MAX!");
        }
    }

    public void UpgradeSkill4()
    {
        if (PlayerStats.instance.skill4Level < PlayerStats.instance.maxSkillLevel && 
            PlayerStats.instance.gold >= PlayerStats.instance.skill4UpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.skill4UpgradeCost;
            PlayerStats.instance.skill4Level++;

            // Pola kelipatan: Lvl 0->1=500, Lvl 1->2=1000, Lvl 2->3=2000, dst.
            PlayerStats.instance.skill4UpgradeCost = 500 * Mathf.RoundToInt(Mathf.Pow(2, PlayerStats.instance.gold >= 0 ? PlayerStats.instance.skill4Level : 0));
            
            Debug.Log("Skill 4 (Slam Attack) Berhasil Upgrade! Level: " + PlayerStats.instance.skill4Level);
        }
    }
}