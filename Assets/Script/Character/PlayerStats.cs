using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    [HideInInspector] public Transform playerBody;
    
    [Header("Status yang Disimpan")]
    public float currentHealth;
    public float maxHealth = 10f;
    public float maxMP = 10f;      
    public float currentMP;        
    public float mpRegenRate = 1f; 
    public float maxEnergy = 5f;
    public float currentEnergy;
    public float energyRegenRate = 1f; 
    public float damage = 1f;
    public float speed = 5.0f;
    public int gold = 0;

    [Header("Inventory Potion")]
    public int smallPotionCount = 0;
    public int mediumPotionCount= 0;
    public int largePotionCount = 0;
    public int smallMPCount = 0;
    public int energyPotionCount = 0;
    public int strengthPotionCount = 0;
    public int speedPotionCount = 0;
    
    [Tooltip("Jumlah HP yang dipulihkan Potion Heal")] 
    public float smallHealAmount = 10f;
    public float mediumHealAmount = 100f;
    public float largeHealAmount = 1000f; 

    [Tooltip("Jumlah MP yang dipulihkan Potion Mana")] 
    public float smallMPAmount = 10f;

    [Header("Rage Settings")]
    public float currentRage = 0f;
    public float maxRage = 100f;
    public bool isRageMode = false;

    [Header("Upgrade Levels (Mulai dari 0)")]
    public int healthLevel = 0;
    public int mpLevel = 0;
    public int energyLevel = 0;
    public int damageLevel = 0;
    public int speedLevel = 0;

    [Header("Base Upgrade Costs (Harga Awal Saat Level 0 adalah 10)")]
    public int healthBaseCost = 10;
    public int mpBaseCost = 10;
    public int energyBaseCost = 20;
    public int damageBaseCost = 10;
    public int speedBaseCost = 20;

    public int healthUpgradeCost => Mathf.RoundToInt(healthBaseCost * Mathf.Pow(1.3f, healthLevel));
    public int mpUpgradeCost => Mathf.RoundToInt(mpBaseCost * Mathf.Pow(1.3f, mpLevel));
    public int energyUpgradeCost => Mathf.RoundToInt(energyBaseCost * Mathf.Pow(1.3f, energyLevel));
    public int damageUpgradeCost => Mathf.RoundToInt(damageBaseCost * Mathf.Pow(1.3f, damageLevel));
    public int speedUpgradeCost => Mathf.RoundToInt(speedBaseCost * Mathf.Pow(1.3f, speedLevel));

    [HideInInspector] public int mpUpgradeCount = 0;

    [Header("Skills Level & Data")]
    public int skill1Level = 0;
    public int skill2Level = 0;
    public int maxSkillLevel = 5;

    [Header("Skill MP Costs")]
    public float skill1MpCost = 3f;
    public float skill2MpCost = 4f;

    [Header("Skill Upgrade Costs")]
    public int skill1UpgradeCost = 10;
    public int skill2UpgradeCost = 150;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 

            LoadStats();
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    void Update()
    {
        if (currentMP < maxMP)
        {
            currentMP += mpRegenRate * Time.deltaTime;
            currentMP = Mathf.Clamp(currentMP, 0, maxMP);
        }

        if (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        }
    }

    public void SaveStats()
    {
        PlayerPrefs.SetFloat("CurrentHealth", currentHealth);
        PlayerPrefs.SetFloat("MaxHealth", maxHealth);
        PlayerPrefs.SetFloat("CurrentMP", currentMP);
        PlayerPrefs.SetFloat("MaxMP", maxMP);
        PlayerPrefs.SetFloat("MpRegenRate", mpRegenRate);
        PlayerPrefs.SetFloat("MaxEnergy", maxEnergy);
        PlayerPrefs.SetFloat("CurrentEnergy", currentEnergy);
        PlayerPrefs.SetFloat("EnergyRegenRate", energyRegenRate);
        PlayerPrefs.SetFloat("Damage", damage);
        PlayerPrefs.SetFloat("Speed", speed);
        PlayerPrefs.SetInt("Gold", gold);

        // Save Inventory Potion
        PlayerPrefs.SetInt("SmallPotionCount", smallPotionCount);
        PlayerPrefs.SetInt("MediumPotionCount", mediumPotionCount);
        PlayerPrefs.SetInt("LargePotionCount", largePotionCount);
        PlayerPrefs.SetInt("SmallMPCount", smallMPCount);
        PlayerPrefs.SetInt("EnergyPotionCount", energyPotionCount);
        PlayerPrefs.SetInt("StrengthPotionCount", strengthPotionCount);
        PlayerPrefs.SetInt("SpeedPotionCount", speedPotionCount);

        // Save Upgrade Levels & Skill
        PlayerPrefs.SetInt("HealthLevel", healthLevel);
        PlayerPrefs.SetInt("MpLevel", mpLevel);
        PlayerPrefs.SetInt("EnergyLevel", energyLevel);
        PlayerPrefs.SetInt("DamageLevel", damageLevel);
        PlayerPrefs.SetInt("SpeedLevel", speedLevel);
        PlayerPrefs.SetInt("MpUpgradeCount", mpUpgradeCount);
        PlayerPrefs.SetInt("Skill1Level", skill1Level);
        PlayerPrefs.SetInt("Skill2Level", skill2Level);

        PlayerPrefs.Save(); // Mengunci dan mengamankan data ke penyimpanan perangkat
        Debug.Log("<color=cyan>SAVE SYSTEM: Progres statistik player berhasil DISIMPAN!</color>");
    }

    public void LoadStats()
    {
        // Parameter kedua di dalam GetFloat/GetInt adalah nilai default jika data belum pernah disimpan sebelumnya
        maxHealth = PlayerPrefs.GetFloat("MaxHealth", 10f);
        currentHealth = PlayerPrefs.GetFloat("CurrentHealth", maxHealth);
        maxMP = PlayerPrefs.GetFloat("MaxMP", 10f);
        currentMP = PlayerPrefs.GetFloat("CurrentMP", maxMP);
        mpRegenRate = PlayerPrefs.GetFloat("MpRegenRate", 1f);
        maxEnergy = PlayerPrefs.GetFloat("MaxEnergy", 5f);
        currentEnergy = PlayerPrefs.GetFloat("CurrentEnergy", maxEnergy);
        energyRegenRate = PlayerPrefs.GetFloat("EnergyRegenRate", 1f);
        damage = PlayerPrefs.GetFloat("Damage", 1f);
        speed = PlayerPrefs.GetFloat("Speed", 5.0f);
        gold = PlayerPrefs.GetInt("Gold", 0);

        // Load Inventory Potion
        smallPotionCount = PlayerPrefs.GetInt("SmallPotionCount", 0);
        mediumPotionCount = PlayerPrefs.GetInt("MediumPotionCount", 0);
        largePotionCount = PlayerPrefs.GetInt("LargePotionCount", 0);
        smallMPCount = PlayerPrefs.GetInt("SmallMPCount", 0);
        energyPotionCount = PlayerPrefs.GetInt("EnergyPotionCount", 0);
        strengthPotionCount = PlayerPrefs.GetInt("StrengthPotionCount", 0);
        speedPotionCount = PlayerPrefs.GetInt("SpeedPotionCount", 0);

        // Load Upgrade Levels & Skill
        healthLevel = PlayerPrefs.GetInt("HealthLevel", 0);
        mpLevel = PlayerPrefs.GetInt("MpLevel", 0);
        energyLevel = PlayerPrefs.GetInt("EnergyLevel", 0);
        damageLevel = PlayerPrefs.GetInt("DamageLevel", 0);
        speedLevel = PlayerPrefs.GetInt("SpeedLevel", 0);
        mpUpgradeCount = PlayerPrefs.GetInt("MpUpgradeCount", 0);
        skill1Level = PlayerPrefs.GetInt("Skill1Level", 0);
        skill2Level = PlayerPrefs.GetInt("Skill2Level", 0);

        Debug.Log("<color=cyan>SAVE SYSTEM: Progres statistik player berhasil DIMUAT!</color>");
    }

    public void ResetStats()
    {
        // 1. Kembalikan batas maksimum status ke nilai awal game
        maxHealth = 10f;
        maxMP = 10f;
        maxEnergy = 5f;
        damage = 1f;
        speed = 5.0f;

        // === TAMBAHKAN INI: Reset level kembali ke 0 ===
        healthLevel = 0;
        mpLevel = 0;
        energyLevel = 0;
        damageLevel = 0;
        speedLevel = 0;

        // 2. Isi penuh statusnya
        currentHealth = maxHealth;
        currentMP = maxMP;
        currentEnergy = maxEnergy;
        
        currentRage = 0f;
        isRageMode = false;

        // 3. Hanguskan semua barang bawaan dan progresi
        gold = 0;
        smallPotionCount = 0;
        mediumPotionCount = 0;
        largePotionCount = 0;
        smallMPCount = 0;
        energyPotionCount = 0;
        strengthPotionCount = 0;
        speedPotionCount = 0;

        skill1Level = 0;
        skill2Level = 0;
        mpUpgradeCount = 0; // Reset juga hitungan bonus mp regen

        Debug.Log("PLAYER STATS: Reset Total! Semua upgrade dan item hangus karena player mati");
    }
}