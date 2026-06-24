using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Ubah Tampilan Kontainer UI")]
    public GameObject potionSlotsContainer;
    public GameObject skillSlotsContainer;
    
    [Header("UI Teks Slot Item")]
    public TextMeshProUGUI smallPotionText;   // Button 1
    public TextMeshProUGUI mediumPotionText;  // Button 2
    public TextMeshProUGUI largePotionText;   // Button 3
    public TextMeshProUGUI smallMPText;       // Button 4
    public TextMeshProUGUI mediumMPText;      // Button 5 (BARU)
    public TextMeshProUGUI largeMPText;       // Button 6 (BARU)
    public TextMeshProUGUI energyPotionText;  // Button 7 (Bergeser)
    public TextMeshProUGUI strengthPotionText;// Button 8 (Bergeser)
    public TextMeshProUGUI speedPotionText;   // Button 9 (Bergeser)

    [Header("UI Teks Slot Skill (Baru)")]
    public TextMeshProUGUI skill1LevelText; 
    public TextMeshProUGUI skill2LevelText; 
    private bool isSkillMode = false;

    void Start()
    {
        if (potionSlotsContainer != null) potionSlotsContainer.SetActive(true);
        if (skillSlotsContainer != null) skillSlotsContainer.SetActive(false);
    }
    
    void Update()
    {
        if (PlayerStats.instance == null) return;

        // 1. FITUR SWITCH: Tekan tombol 'E' untuk ganti slot
        if (Input.GetKeyDown(KeyCode.E))
        {
            isSkillMode = !isSkillMode; 
            potionSlotsContainer.SetActive(!isSkillMode);
            skillSlotsContainer.SetActive(isSkillMode);
            Debug.Log("Switch Slot Mode! Mode Skill: " + isSkillMode);
        }

        // 2. Sinkronisasi Teks Jumlah Potion & Level Skill ke UI
        UpdateUITexts();

        // 3. Eksekusi Hotkey berdasarkan Mode Terpilih (Kini Mendukung Hotkey 1 - 9)
        PlayerMovement pm = PlayerStats.instance.playerBody.GetComponent<PlayerMovement>();
        if (pm == null) return;

        if (!isSkillMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) pm.TriggerPotionAnimation(1);  // Small HP
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) pm.TriggerPotionAnimation(2);  // Medium HP
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) pm.TriggerPotionAnimation(3);  // Large HP
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) pm.TriggerPotionAnimation(4);  // Small MP
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) pm.TriggerPotionAnimation(5);  // Medium MP (BARU)
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) pm.TriggerPotionAnimation(6);  // Large MP (BARU)
            if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) pm.TriggerPotionAnimation(7);  // Energy
            if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8)) pm.TriggerPotionAnimation(8);  // Strength
            if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9)) pm.TriggerPotionAnimation(9);  // Speed
        }
    }

    void UpdateUITexts()
    {
        // Potion HP & MP
        if (smallPotionText != null) smallPotionText.text = "x" + PlayerStats.instance.smallPotionCount;
        if (mediumPotionText != null) mediumPotionText.text = "x" + PlayerStats.instance.mediumPotionCount;
        if (largePotionText != null) largePotionText.text = "x" + PlayerStats.instance.largePotionCount;
        if (smallMPText != null) smallMPText.text = "x" + PlayerStats.instance.smallMPCount;
        if (mediumMPText != null) mediumMPText.text = "x" + PlayerStats.instance.mediumMPCount; // BARU
        if (largeMPText != null) largeMPText.text = "x" + PlayerStats.instance.largeMPCount;   // BARU
        
        // Potion Buff
        if (energyPotionText != null) energyPotionText.text = "x" + PlayerStats.instance.energyPotionCount;
        if (strengthPotionText != null) strengthPotionText.text = "x" + PlayerStats.instance.strengthPotionCount;
        if (speedPotionText != null) speedPotionText.text = "x" + PlayerStats.instance.speedPotionCount;

        // Skill
        if (skill1LevelText != null) 
            skill1LevelText.text = PlayerStats.instance.skill1Level > 0 ? "Lv. " + PlayerStats.instance.skill1Level : "LOCK";
        
        if (skill2LevelText != null) 
            skill2LevelText.text = PlayerStats.instance.skill2Level > 0 ? "Lv. " + PlayerStats.instance.skill2Level : "LOCK";
    }
}