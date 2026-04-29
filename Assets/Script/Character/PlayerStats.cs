using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    [HideInInspector] public Transform playerBody;
    
    [Header("Status yang Disimpan")]
    public float currentHealth;
    public float maxHealth = 10f;
    public float maxMP = 10f;      // Tambahkan ini
    public float currentMP;        // Tambahkan ini
    public float mpRegenRate = 1f; // MP pulih 2 poin per detik
    public float maxEnergy = 5f;
    public float currentEnergy;
    public float energyRegenRate = 1f; // Energy pulih lebih cepat
    public float damage = 1f;
    public float speed = 5.0f;
    public int gold = 0;

    [Header("Upgrade Costs")]
    public int healthUpgradeCost = 1;
    public int mpUpgradeCost = 1;
    public int energyUpgradeCost = 50;
    public int damageUpgradeCost = 1;
    public int speedUpgradeCost = 20;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // JANGAN hapus saat pindah Scene

            // Inisialisasi status saat pertama kali load
            currentHealth = maxHealth;
            currentMP = maxMP; // Set MP penuh di awal game
            currentEnergy = maxEnergy;
        }
        else
        {
            Destroy(gameObject); // Hapus jika ada duplikat di Scene baru
        }
    }

    void Update()
    {
        // Regenerasi MP otomatis setiap detik
        if (currentMP < maxMP)
        {
            currentMP += mpRegenRate * Time.deltaTime;
            
            // Memastikan MP tidak meluap melebihi maxMP
            currentMP = Mathf.Clamp(currentMP, 0, maxMP);
        }

        // Regenerasi Energy otomatis
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        }
    }
}