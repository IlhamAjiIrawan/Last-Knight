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

    // --- TAMBAHAN FITUR: INVENTORY POTION ---
    [Header("Inventory Potion")]
    public int smallPotionCount = 0;
    public int mediumPotionCount= 0;
    public int largePotionCount = 0;
    public int smallMPCount = 0;
    public int energyPotionCount = 0;
    public int strengthPotionCount = 0;
    public int speedPotionCount = 0;
    
    [Tooltip("Jumlah HP yang dipulihkan Potion Heal")] 
    public float smallHealAmount = 10f;  // Sembuh 3 darah
    public float mediumHealAmount = 100f;
    public float largeHealAmount = 1000f;  // Sembuh 7 darah

    [Tooltip("Jumlah MP yang dipulihkan Potion Mana")] 
    public float smallMPAmount = 10f;

    [Header("Rage Settings")]
    public float currentRage = 0f;
    public float maxRage = 100f;
    public bool isRageMode = false;

    [Header("Upgrade Costs")]
    public int healthUpgradeCost = 1;
    public int mpUpgradeCost = 1;
    public int energyUpgradeCost = 50;
    public int damageUpgradeCost = 1;
    public int speedUpgradeCost = 20;

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

            currentHealth = maxHealth;
            currentMP = maxMP; 
            currentEnergy = maxEnergy;
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
}