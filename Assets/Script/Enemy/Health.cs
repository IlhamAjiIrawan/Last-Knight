using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public Slider healthSlider; // Tarik slider musuh ke sini
    public float maxHealth = 10f;
    private float currentHealth;
    private Animator anim;
    private bool isDead = false;

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

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        // Cek status immune dari PlayerMovement
        if (gameObject.CompareTag("Player"))
        {
            PlayerMovement pm = GetComponent<PlayerMovement>();
            if (pm != null && pm.isImmune) // Menggunakan isImmune, bukan isDashing
            {
                Debug.Log("Serangan Terhindar! (Immune)");
                return; 
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
            anim.SetTrigger("getHit"); // Player tetap putar animasi hit
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
        // Sembunyikan health bar saat mati agar tidak melayang di mayat
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        anim.ResetTrigger("getHit");
        anim.SetTrigger("die"); 

        // LOGIKA DROP ITEM
        if (itemToDrop != null)
        {
            for (int i = 0; i < amountToDrop; i++)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(-dropSpread, dropSpread),
                    0,
                    Random.Range(-dropSpread, dropSpread)
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