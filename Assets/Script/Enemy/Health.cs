using UnityEngine;
using UnityEngine.UI;
using System;

public class Health : MonoBehaviour
{
    public Slider healthSlider; // Tarik slider musuh ke sini
    public float maxHealth = 10f;
    private float currentHealth;
    private Animator anim;
    private bool isDead = false;
    public Action onDeath;

    [Header("Drop Settings")]
    public GameObject itemToDrop; // Prefab mata uang
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
            currentHealth = maxHealth; // Musuh tetap pakai nilai sendiri
        }
        anim = GetComponent<Animator>();

        // Inisialisasi Slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // Tambahkan ini di Health.cs
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
                // 2. Terhindar karena Dash (Immune)
                if (pm.isImmune)
                {
                    Debug.Log("Serangan Terhindar! (Immune)");
                    return; 
                }

                // --- TAMBAHKAN KODE BARU DI SINI ---
                // 3. Menahan serangan menggunakan Block Perisai
                if (pm.AbsorbDamageWithBlock(damage))
                {
                    // Jika ingin memicu animasi perisai terpukul, kamu bisa menambahkan: anim.SetTrigger("blockHit");
                    Debug.Log("Damage diserap sepenuhnya oleh perisai!");
                    return; // Keluar fungsi awal agar darah & animasi getHit tidak jalan
                }
            }
        }
        currentHealth -= damage;

        //Update Silder Enemy
        if (gameObject.CompareTag("Enemy") && healthSlider != null)
        {
            // Munculkan health bar jika sebelumnya disembunyikan
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
        else if (gameObject.CompareTag("Enemy"))
        {
            // Panggil fungsi TakeHit di EnemyAI agar dia diam
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.TakeHit();
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
        if (gameObject.CompareTag("Enemy"))
        {
            PlayerStats.instance.currentRage += 1f; // Tambah 1 poin
            // Pastikan tidak melebihi 100
            PlayerStats.instance.currentRage = Mathf.Clamp(PlayerStats.instance.currentRage, 0, 100);
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

        // 1. Matikan script AI agar Update() berhenti berjalan
        if (GetComponent<EnemyAI>())
            GetComponent<EnemyAI>().enabled = false;

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

        if (gameObject.CompareTag("Enemy"))
            Destroy(gameObject, 5f);
    }
}