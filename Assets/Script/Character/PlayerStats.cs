using UnityEngine;
using System.IO; // PENTING: Ditambahkan untuk mengaktifkan fitur baca & tulis file fisik
using UnityEngine.SceneManagement; // BARU: Untuk membaca nama scene secara otomatis

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

    [HideInInspector] public string lastSavedScene = "Map1Village"; // BARU: Menampung nama scene aktif secara runtime

    [Header("Inventory Potion")]
    public int smallPotionCount = 0;
    public int mediumPotionCount= 0;
    public int largePotionCount = 0;
    public int smallMPCount = 0;
    public int mediumMPCount = 0; // BARU: Tambahan Potion MP Medium
    public int largeMPCount = 0;  // BARU: Tambahan Potion MP Large
    public int energyPotionCount = 0;
    public int strengthPotionCount = 0;
    public int speedPotionCount = 0;
    
    [Tooltip("Jumlah HP yang dipulihkan Potion Heal")] 
    public float smallHealAmount = 10f;
    public float mediumHealAmount = 100f;
    public float largeHealAmount = 1000f; 

    [Tooltip("Jumlah MP yang dipulihkan Potion Mana")] 
    public float smallMPAmount = 10f;
    public float mediumMPAmount = 100f; // BARU: Nilai pemulihan MP Medium
    public float largeMPAmount = 1000f;  // BARU: Nilai pemulihan MP Large

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
    public int skill3Level = 0;
    public int maxSkillLevel = 5;

    [Header("Skill MP Costs")]
    public float skill1MpCost = 3f;
    public float skill2MpCost = 4f;

    [Header("Skill Upgrade Costs")]
    public int skill1UpgradeCost = 10;
    public int skill2UpgradeCost = 150;
    public int skill3UpgradeCost = 250;

    public float skill3MpCost => skill3Level * 25f;
    public float skill3DamageMultiplier => skill3Level * 2.5f;
    public float skill3ScaleMultiplier => skill3Level * 1f;

    private string saveFileName = "save_player_data.json";
    private string savePath;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);

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

    [System.Serializable]
    public class PlayerData
    {
        public string lastSavedScene; 
        public float currentHealth;
        public float maxHealth;
        public float currentMP;
        public float maxMP;
        public float mpRegenRate;
        public float maxEnergy;
        public float currentEnergy;
        public float energyRegenRate;
        public float damage;
        public float speed;
        public int gold;

        public int smallPotionCount;
        public int mediumPotionCount;
        public int largePotionCount;
        public int smallMPCount;
        public int mediumMPCount; // BARU: Untuk JSON Save
        public int largeMPCount;  // BARU: Untuk JSON Save
        public int energyPotionCount;
        public int strengthPotionCount;
        public int speedPotionCount;

        public int healthLevel;
        public int mpLevel;
        public int energyLevel;
        public int damageLevel;
        public int speedLevel;
        public int mpUpgradeCount;
        public int skill1Level;
        public int skill2Level;
        public int skill3Level; 
        public int skill3UpgradeCost;
    }

    public void SaveStats(string customSceneName = "")
    {
        PlayerData data = new PlayerData();
        
        if (!string.IsNullOrEmpty(customSceneName))
        {
            data.lastSavedScene = customSceneName;
        }
        else
        {
            data.lastSavedScene = SceneManager.GetActiveScene().name;
        }

        data.currentHealth = currentHealth;
        data.maxHealth = maxHealth;
        data.currentMP = currentMP;
        data.maxMP = maxMP;
        data.mpRegenRate = mpRegenRate;
        data.maxEnergy = maxEnergy;
        data.currentEnergy = currentEnergy;
        data.energyRegenRate = energyRegenRate;
        data.damage = damage;
        data.speed = speed;
        data.gold = gold;

        data.smallPotionCount = smallPotionCount;
        data.mediumPotionCount = mediumPotionCount;
        data.largePotionCount = largePotionCount;
        data.smallMPCount = smallMPCount;
        data.mediumMPCount = mediumMPCount; // BARU
        data.largeMPCount = largeMPCount;   // BARU
        data.energyPotionCount = energyPotionCount;
        data.strengthPotionCount = strengthPotionCount;
        data.speedPotionCount = speedPotionCount;

        data.healthLevel = healthLevel;
        data.mpLevel = mpLevel;
        data.energyLevel = energyLevel;
        data.damageLevel = damageLevel;
        data.speedLevel = speedLevel;
        data.mpUpgradeCount = mpUpgradeCount;
        data.skill1Level = skill1Level;
        data.skill2Level = skill2Level;
        data.skill3Level = skill3Level;           
        data.skill3UpgradeCost = skill3UpgradeCost;

        string jsonText = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, jsonText);

        Debug.Log("<color=lime>[Save System]: Data & Lokasi Scene berhasil ditulis ke file: </color>" + savePath);
    }

    public void LoadStats()
    {
        if (File.Exists(savePath))
        {
            string jsonText = File.ReadAllText(savePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(jsonText);

            lastSavedScene = data.lastSavedScene;
            if (string.IsNullOrEmpty(lastSavedScene))
            {
                lastSavedScene = "Map1Village";
            }

            maxHealth = data.maxHealth;
            currentHealth = data.currentHealth;
            maxMP = data.maxMP;
            currentMP = data.currentMP;
            mpRegenRate = data.mpRegenRate;
            maxEnergy = data.maxEnergy;
            currentEnergy = data.currentEnergy;
            energyRegenRate = data.energyRegenRate;
            damage = data.damage;
            speed = data.speed;
            gold = data.gold;

            smallPotionCount = data.smallPotionCount;
            mediumPotionCount = data.mediumPotionCount;
            largePotionCount = data.largePotionCount;
            smallMPCount = data.smallMPCount;
            mediumMPCount = data.mediumMPCount; // BARU
            largeMPCount = data.largeMPCount;   // BARU
            energyPotionCount = data.energyPotionCount;
            strengthPotionCount = data.strengthPotionCount;
            speedPotionCount = data.speedPotionCount;

            healthLevel = data.healthLevel;
            mpLevel = data.mpLevel;
            energyLevel = data.energyLevel;
            damageLevel = data.damageLevel;
            speedLevel = data.speedLevel;
            mpUpgradeCount = data.mpUpgradeCount;
            skill1Level = data.skill1Level;
            skill2Level = data.skill2Level;
            skill3Level = data.skill3Level;
            skill3UpgradeCost = data.skill3UpgradeCost;
            if (skill3UpgradeCost <= 0)
            {
                skill3UpgradeCost = 250; // Atur sesuai harga awal yang kamu inginkan
            }

            Debug.Log("<color=lime>[Save System]: Berhasil memuat status player dari file JSON.</color>");
        }
        else
        {
            Debug.LogWarning("[Save System]: File simpanan JSON belum ada. Menggunakan status default.");
            SetDefaultStats();
        }
    }

    private void SetDefaultStats()
    {
        lastSavedScene = "Map1Village"; 
        maxHealth = 10f;
        maxMP = 10f;
        maxEnergy = 5f;
        damage = 1f;
        speed = 5.0f;

        healthLevel = 0;
        mpLevel = 0;
        energyLevel = 0;
        damageLevel = 0;
        speedLevel = 0;

        currentHealth = maxHealth;
        currentMP = maxMP;
        currentEnergy = maxEnergy;
        
        currentRage = 0f;
        isRageMode = false;

        gold = 0;
        smallPotionCount = 0;
        mediumPotionCount = 0;
        largePotionCount = 0;
        smallMPCount = 0;
        mediumMPCount = 0; // BARU
        largeMPCount = 0;  // BARU
        energyPotionCount = 0;
        strengthPotionCount = 0;
        speedPotionCount = 0;

        skill1Level = 0;
        skill2Level = 0;
        skill3Level = 0;              // BARU
        skill3UpgradeCost = 250;
        mpUpgradeCount = 0;
    }

    public void ResetStats()
    {
        SetDefaultStats();

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("<color=red>[Save System]: File save lama di harddisk telah sukses DIHAPUS.</color>");
        }
    }
}