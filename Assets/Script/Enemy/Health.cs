using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 10f;
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

        anim.ResetTrigger("getHit");
        anim.SetTrigger("die"); 

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