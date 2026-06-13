using UnityEngine;

// Memastikan objek ini otomatis memiliki Rigidbody saat dimasukkan ke Prefab
[RequireComponent(typeof(Rigidbody))]
public class KnifeProjectile : MonoBehaviour
{
    public float damage = 15f;
    public float speed = 20f;       // Kecepatan terbang pisau
    public float lifetime = 3f;     // Batas waktu pisau hancur jika meleset (tidak kena apapun)

    private bool hasDamagedPlayer = false; 
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Abaikan tabrakan jika pisau tidak sengaja menyentuh tubuh Boss/Enemy lain saat dilempar
        if (other.CompareTag("Boss") || other.CompareTag("Enemy")) return;

        // 1. Cek apakah objek yang ditabrak adalah Player dan belum pernah diberi damage oleh objek ini
        if (other.CompareTag("Player") && !hasDamagedPlayer)
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                // 2. Berikan damage ke Player
                playerHealth.TakeDamage(damage);
                
                hasDamagedPlayer = true; 

                // 4. Hancurkan objek pisau setelah berhasil melukai Player
                Destroy(gameObject);
            }
        }
        
        else if (other.CompareTag("Environment") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Jika pisau menabrak dinding, lantai, atau rintangan map, langsung hancur
            //Destroy(gameObject);
        }
        
    }
}