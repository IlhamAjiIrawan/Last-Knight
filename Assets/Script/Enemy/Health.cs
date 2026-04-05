using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private Animator anim;
    private bool isDead = false;

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
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    
        // Jika Player, simpan perubahan nyawa ke PlayerStats
        if (gameObject.CompareTag("Player"))
        {
            PlayerStats.instance.currentHealth = currentHealth;
        }

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. Jalankan Trigger Animasi
        anim.SetTrigger("die"); 

        // 2. Matikan Komponen Kontrol
        if (GetComponent<PlayerMovement>())
            GetComponent<PlayerMovement>().enabled = false;

        // 3. Matikan Fisika & Navigasi
        // Agar karakter tidak didorong-dorong musuh saat sudah jadi mayat
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().isKinematic = true;

        if (GetComponent<UnityEngine.AI.NavMeshAgent>())
            GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

        // 4. Matikan Collider (Sangat Penting!)
        // Supaya musuh tidak tersangkut di mayat karakter kamu
        if (GetComponent<Collider>())
            GetComponent<Collider>().enabled = false;

        Debug.Log(gameObject.name + " telah mencapai Dead Pose.");
    }
}