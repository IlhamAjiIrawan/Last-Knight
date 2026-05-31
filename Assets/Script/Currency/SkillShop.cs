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
            
            // Menaikkan harga upgrade berikutnya (skala x1.5)
            PlayerStats.instance.skill1UpgradeCost = Mathf.RoundToInt(PlayerStats.instance.skill1UpgradeCost * 1.5f);
            Debug.Log("Skill 1 naik ke Level: " + PlayerStats.instance.skill1Level);
        }
    }

    public void UpgradeSkill2()
    {
        if (PlayerStats.instance.skill2Level < PlayerStats.instance.maxSkillLevel && 
            PlayerStats.instance.gold >= PlayerStats.instance.skill2UpgradeCost)
        {
            PlayerStats.instance.gold -= PlayerStats.instance.skill2UpgradeCost;
            PlayerStats.instance.skill2Level++;

            PlayerStats.instance.skill2UpgradeCost = Mathf.RoundToInt(PlayerStats.instance.skill2UpgradeCost * 1.5f);
            Debug.Log("Skill 2 naik ke Level: " + PlayerStats.instance.skill2Level);
        }
    }
}