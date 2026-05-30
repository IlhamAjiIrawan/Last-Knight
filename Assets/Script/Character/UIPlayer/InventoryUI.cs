using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Teks Slot Item")]
    public TextMeshProUGUI smallPotionText; // Tarik Text Slot angka 1 ke sini
    public TextMeshProUGUI mediumPotionText;
    public TextMeshProUGUI largePotionText; // Tarik Text Slot angka 2 ke sini
    public TextMeshProUGUI smallMPText;     // Slot 3 (Baru)
    public TextMeshProUGUI energyPotionText; // Slot 4 (Baru)
    public TextMeshProUGUI strengthPotionText;// Slot 5 (Baru)
    public TextMeshProUGUI speedPotionText;  // Slot 6 (Baru)

    void Update()
    {
        if (PlayerStats.instance == null) return;

        // 1. Sinkronisasi angka jumlah item ke teks UI
        if (smallPotionText != null) smallPotionText.text = "x" + PlayerStats.instance.smallPotionCount;
        if (mediumPotionText != null) mediumPotionText.text = "x" + PlayerStats.instance.mediumPotionCount;
        if (largePotionText != null) largePotionText.text = "x" + PlayerStats.instance.largePotionCount;
        if (smallMPText != null) smallMPText.text = "x" + PlayerStats.instance.smallMPCount;
        if (energyPotionText != null) energyPotionText.text = "x" + PlayerStats.instance.energyPotionCount;
        if (strengthPotionText != null) strengthPotionText.text = "x" + PlayerStats.instance.strengthPotionCount;
        if (speedPotionText != null) speedPotionText.text = "x" + PlayerStats.instance.speedPotionCount;

        PlayerMovement pm = PlayerStats.instance.playerBody.GetComponent<PlayerMovement>();
        if (pm == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) pm.TriggerPotionAnimation(1);  // Picu animasi Small Potion
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) pm.TriggerPotionAnimation(2);  // Picu animasi Medium Potion
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) pm.TriggerPotionAnimation(3); // Picu animasi Large Potion
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) pm.TriggerPotionAnimation(4);
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) pm.TriggerPotionAnimation(5);
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) pm.TriggerPotionAnimation(6);
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) pm.TriggerPotionAnimation(7);
        
    }
}