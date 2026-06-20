using UnityEngine;
using System.IO; // PENTING: Ditambahkan untuk mengaktifkan fitur baca & tulis file fisik

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

    // Variabel baru untuk menentukan nama file dan lokasi folder penyimpanan
    private string saveFileName = "save_player_data.json";
    private string savePath;

    void Awake()
    {
        // Tentukan path folder simpanan otomatis berdasarkan OS perangkat (Windows/Android/iOS)
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
    }

    public void SaveStats()
    {
        PlayerData data = new PlayerData();
        
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

        // Convert data objek menjadi teks string berformat JSON
        string jsonText = JsonUtility.ToJson(data, true);

        // Tulis teks tersebut menjadi file fisik di penyimpanan local
        File.WriteAllText(savePath, jsonText);

        Debug.Log("<color=lime>[Save System]: Data berhasil ditulis ke file: </color>" + savePath);
    }

    public void LoadStats()
    {
        // Cek apakah file simpanan berformat .json tersebut ada di direktori
        if (File.Exists(savePath))
        {
            string jsonText = File.ReadAllText(savePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(jsonText);

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

            Debug.Log("<color=lime>[Save System]: Berhasil memuat status player dari file JSON.</color>");
        }
        else
        {
            // Jika file tidak ditemukan (Game baru pertama kali dimainkan), jalankan status default bawaan
            Debug.LogWarning("[Save System]: File simpanan JSON belum ada. Menggunakan status default.");
            SetDefaultStats();
        }
    }

    // Fungsi pembantu untuk mengatur data default saat game baru dimulai pertama kali
    private void SetDefaultStats()
    {
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
        energyPotionCount = 0;
        strengthPotionCount = 0;
        speedPotionCount = 0;

        skill1Level = 0;
        skill2Level = 0;
        mpUpgradeCount = 0;
    }

    public void ResetStats()
    {
        SetDefaultStats();

        // Tambahan: Hapus file .json fisik jika player memilih opsi reset total data
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("<color=red>[Save System]: File save lama di harddisk telah sukses DIHAPUS.</color>");
        }
    }
}