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