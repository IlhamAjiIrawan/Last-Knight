using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Teks Slot Item")]
    public TextMeshProUGUI smallPotionText; // Tarik Text Slot angka 1 ke sini
    public TextMeshProUGUI largePotionText; // Tarik Text Slot angka 2 ke sini

    void Update()
    {
        if (PlayerStats.instance == null) return;

        // 1. Sinkronisasi angka jumlah item ke teks UI
        if (smallPotionText != null)
            smallPotionText.text = "x" + PlayerStats.instance.smallPotionCount;

        if (largePotionText != null)
            largePotionText.text = "x" + PlayerStats.instance.largePotionCount;

        // 2. Gunakan Item dengan Tombol Angka 1 & 2
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            UsePotion(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            UsePotion(false);
        }
    }

    void UsePotion(bool isSmall)
    {
        // Cek apakah darah player sudah penuh atau player sudah mati
        if (PlayerStats.instance.currentHealth >= PlayerStats.instance.maxHealth || PlayerStats.instance.currentHealth <= 0) 
            return;

        if (isSmall && PlayerStats.instance.smallPotionCount > 0)
        {
            PlayerStats.instance.smallPotionCount--;
            PlayerStats.instance.currentHealth += PlayerStats.instance.smallHealAmount;
            Debug.Log("Menggunakan Small Potion");
        }
        else if (!isSmall && PlayerStats.instance.largePotionCount > 0)
        {
            PlayerStats.instance.largePotionCount--;
            PlayerStats.instance.currentHealth += PlayerStats.instance.largeHealAmount;
            Debug.Log("Menggunakan Large Potion");
        }

        // Batasi agar penambahan darah tidak meluap melebihi maxHealth
        PlayerStats.instance.currentHealth = Mathf.Clamp(PlayerStats.instance.currentHealth, 0f, PlayerStats.instance.maxHealth);
    }
}