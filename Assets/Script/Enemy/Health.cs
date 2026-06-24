using UnityEngine;
using UnityEngine.UI;
using System;

public class Health : MonoBehaviour
{
    public Slider healthSlider; // Tarik slider musuh/boss ke sini
    public float maxHealth = 10f;

    // --- DIUBAH JADI PUBLIC: Sesuai solusi error CS0122 agar bisa dibaca WaveManager ---
    public float currentHealth; 
    
    private Animator anim;
    private bool isDead = false;
    public Action onDeath;

    [Header("Drop Settings")]
    public GameObject itemToDrop; // Prefab mata uang / koin
    public int amountToDrop = 1;  // Jumlah item yang dijatuhkan
    public float dropSpread = 0.5f; // Jarak pencaran antar koin agar tidak menumpuk di satu titik

    void Start()
    {
        if (gameObject.CompareTag("Player"))
        {
            // Ambil data dari PlayerStats yang abadi
            maxHealth = PlayerStats.instance.maxHealth;
            currentHealth = PlayerStats.instance.currentHealth;
        }
        else
        {
            currentHealth = maxHealth; // Musuh & Boss tetap pakai nilai maxHealth sendiri
        }
        anim = GetComponent<Animator>();

        // Inisialisasi Slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Update()
    {
        if (gameObject.CompareTag("Player"))
        {
            // Selalu sinkronkan variabel lokal dengan data di PlayerStats
            currentHealth = PlayerStats.instance.currentHealth;

            // Jika player punya slider di atas kepalanya, update juga di sini
            if (healthSlider != null) 
            {
                healthSlider.value = currentHealth;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        // Cek status immune dari PlayerMovement
        if (gameObject.CompareTag("Player"))
        {
            if (PlayerStats.instance.isRageMode && UnityEngine.Random.value < 0.5f)
            {
                Debug.Log("Rage Evasion! Serangan meleset.");
                return; 
            }
            
            currentHealth = PlayerStats.instance.currentHealth;
            PlayerMovement pm = GetComponent<PlayerMovement>();
            if (pm != null) 
            {
                // Terhindar karena Dash (Immune)
                if (pm.isImmune)
                {
                    Debug.Log("Serangan Terhindar! (Immune)");
                    return; 
                }

               // === LOGIKA PENGURANGAN DURABILITY SHIELD SKILL 2 ===
                if (pm.isShieldActive)
                {
                    if (pm.currentShieldHp >= damage)
                    {
                        pm.currentShieldHp -= damage;
                        Debug.Log("Damage sebesar " + damage + " diserap Shield. Sisa Shield: " + pm.currentShieldHp);
                        return; 
                    }
                    else
                    {
                        // Jika damage musuh lebih besar dari sisa shield
                        damage -= pm.currentShieldHp; // Sisa damage dikalkulasikan untuk mengurangi HP asli

                        // BARU: Panggil fungsi BreakShield untuk menghancurkan prefab secara instan
                        pm.BreakShield(); 
                        
                        Debug.Log("Skill Shield hancur karena HP tameng habis! Sisa damage tembus ke HP asli: " + damage);
                    }
                }

                // Menahan serangan menggunakan Block Perisai bawaan (Middle Click)
                if (pm.AbsorbDamageWithBlock(damage))
                {
                    Debug.Log("Damage diserap sepenuhnya oleh perisai!");
                    return; 
                }
                pm.OnPlayerHit();
            }
        }

        if (IsTargetEnemy() && PlayerStats.instance != null && !PlayerStats.instance.isRageMode)
        {
            PlayerStats.instance.currentRage += 1f;
            // Batasi langsung dengan Max Rage dari PlayerStats
            PlayerStats.instance.currentRage = Mathf.Clamp(PlayerStats.instance.currentRage, 0f, PlayerStats.instance.maxRage);
            Debug.Log("Serangan masuk! +1 Rage. Total: " + PlayerStats.instance.currentRage);
        }

        currentHealth -= damage;

        // Jika objek adalah musuh, update UI Health Bar-nya
        if (IsTargetEnemy() && healthSlider != null)
        {
            healthSlider.gameObject.SetActive(true); 
            healthSlider.value = currentHealth;
        }
    
        // Jika Player, simpan perubahan nyawa ke PlayerStats
        if (gameObject.CompareTag("Player"))
        {
            PlayerStats.instance.currentHealth = currentHealth;
            if (!PlayerStats.instance.isRageMode)
            {
                anim.SetTrigger("getHit");
            }
        }
        else if (IsTargetEnemy())
        {
            // Panggil fungsi TakeHit di EnemyAI jika objeknya adalah kroco/musuh biasa/heavy enemy
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.TakeHit();
            }

            RangedEnemyAI rangedAi = GetComponent<RangedEnemyAI>();
            if (rangedAi != null)
            {
                rangedAi.TakeHit();
            }
        }

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (onDeath != null) onDeath.Invoke();
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        if (IsTargetEnemy() && PlayerStats.instance != null && !PlayerStats.instance.isRageMode)
        {
            PlayerStats.instance.currentRage += 2f; 
            PlayerStats.instance.currentRage = Mathf.Clamp(PlayerStats.instance.currentRage, 0f, PlayerStats.instance.maxRage);
        }

        anim.ResetTrigger("getHit");
        anim.SetTrigger("die"); 

        // LOGIKA DROP ITEM
        if (itemToDrop != null)
        {
            for (int i = 0; i < amountToDrop; i++)
            {
                Vector3 randomPosition = new Vector3(
                    UnityEngine.Random.Range(-dropSpread, dropSpread),
                    0,
                    UnityEngine.Random.Range(-dropSpread, dropSpread)
                );
                
                Instantiate(itemToDrop, transform.position + Vector3.up + randomPosition, Quaternion.identity);
            }
        }

        // 1. Matikan script AI Kroco agar Update() berhenti berjalan
        if (GetComponent<EnemyAI>())
            GetComponent<EnemyAI>().enabled = false;

        if (GetComponent<RangedEnemyAI>())
            GetComponent<RangedEnemyAI>().enabled = false;

        // --- Matikan script Boss AI RedDragon agar naga berhenti menyerang ---
        if (GetComponent<RedDragon>())
            GetComponent<RedDragon>().enabled = false;

        // 2. Matikan pergerakan player (jika ini player)
        if (GetComponent<PlayerMovement>())
            GetComponent<PlayerMovement>().enabled = false;

        // 3. Matikan NavMeshAgent secara total
        if (GetComponent<UnityEngine.AI.NavMeshAgent>())
            GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

        // 4. Matikan Collider agar tidak menghalangi jalan
        if (GetComponent<Collider>())
            GetComponent<Collider>().enabled = false;

        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().isKinematic = true;

        Debug.Log(gameObject.name + " telah mati.");

        // Hancurkan gameobject setelah 5 detik (berlaku untuk Enemy, HeavyEnemy, dan Boss)
        if (!gameObject.CompareTag("Player"))
            Destroy(gameObject, 5f);
    }

    // --- FUNGSI HELPER AMAN (Mencegah Typo Huruf Kapital di Unity Editor) ---
    private bool IsTargetEnemy()
    {
        int currentLayer = gameObject.layer;
        
        // Memeriksa variasi huruf kapital/kecil agar sistem deteksi UI dan getHit tidak mogok
        return currentLayer == LayerMask.NameToLayer("enemy") || 
               currentLayer == LayerMask.NameToLayer("Enemy") || 
               currentLayer == LayerMask.NameToLayer("HeavyEnemy") || 
               currentLayer == LayerMask.NameToLayer("heavyenemy") || 
               currentLayer == LayerMask.NameToLayer("Boss") || 
               currentLayer == LayerMask.NameToLayer("boss");
    }
}