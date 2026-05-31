using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Ubah Tampilan Kontainer UI")]
    public GameObject potionSlotsContainer;
    public GameObject skillSlotsContainer;
    
    [Header("UI Teks Slot Item")]
    public TextMeshProUGUI smallPotionText; // Tarik Text Slot angka 1 ke sini
    public TextMeshProUGUI mediumPotionText;
    public TextMeshProUGUI largePotionText; // Tarik Text Slot angka 2 ke sini
    public TextMeshProUGUI smallMPText;     // Slot 3 (Baru)
    public TextMeshProUGUI energyPotionText; // Slot 4 (Baru)
    public TextMeshProUGUI strengthPotionText;// Slot 5 (Baru)
    public TextMeshProUGUI speedPotionText;  // Slot 6 (Baru)

    [Header("UI Teks Slot Skill (Baru)")]
    public TextMeshProUGUI skill1LevelText; // Menampilkan Level Skill 1 di slot HUD
    public TextMeshProUGUI skill2LevelText; // Menampilkan Level Skill 2 di slot HUD
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
            isSkillMode = !isSkillMode; // Mengubah true jadi false, atau sebaliknya
            potionSlotsContainer.SetActive(!isSkillMode);
            skillSlotsContainer.SetActive(isSkillMode);
            Debug.Log("Switch Slot Mode! Mode Skill: " + isSkillMode);
        }

        // 2. Sinkronisasi Teks Jumlah Potion & Level Skill ke UI
        UpdateUITexts();

        // 3. Eksekusi Hotkey berdasarkan Mode Terpilih
        PlayerMovement pm = PlayerStats.instance.playerBody.GetComponent<PlayerMovement>();
        if (pm == null) return;

        if (!isSkillMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) pm.TriggerPotionAnimation(1);  // Picu animasi Small Potion
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) pm.TriggerPotionAnimation(2);  // Picu animasi Medium Potion
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) pm.TriggerPotionAnimation(3); // Picu animasi Large Potion
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) pm.TriggerPotionAnimation(4);
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) pm.TriggerPotionAnimation(5);
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) pm.TriggerPotionAnimation(6);
            if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) pm.TriggerPotionAnimation(7);
        }
        else
        {
            // --- MODE SKILL (1-2) ---
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) pm.CastSkill(1);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) pm.CastSkill(2);
        }
    }

    void UpdateUITexts()
    {
        // Potion
         if (smallPotionText != null) smallPotionText.text = "x" + PlayerStats.instance.smallPotionCount;
        if (mediumPotionText != null) mediumPotionText.text = "x" + PlayerStats.instance.mediumPotionCount;
        if (largePotionText != null) largePotionText.text = "x" + PlayerStats.instance.largePotionCount;
        if (smallMPText != null) smallMPText.text = "x" + PlayerStats.instance.smallMPCount;
        if (energyPotionText != null) energyPotionText.text = "x" + PlayerStats.instance.energyPotionCount;
        if (strengthPotionText != null) strengthPotionText.text = "x" + PlayerStats.instance.strengthPotionCount;
        if (speedPotionText != null) speedPotionText.text = "x" + PlayerStats.instance.speedPotionCount;

        // Skill (Menampilkan LV.X atau "LOCK" jika belum dibeli)
        if (skill1LevelText != null) 
            skill1LevelText.text = PlayerStats.instance.skill1Level > 0 ? "Lv. " + PlayerStats.instance.skill1Level : "LOCK";
        
        if (skill2LevelText != null) 
            skill2LevelText.text = PlayerStats.instance.skill2Level > 0 ? "Lv. " + PlayerStats.instance.skill2Level : "LOCK";
    }
}