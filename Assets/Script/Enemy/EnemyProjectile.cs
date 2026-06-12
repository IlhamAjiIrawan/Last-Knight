using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 15f;
    public float lifeTime = 4f; // Hancur otomatis jika tidak mengenai apapun agar hemat memori

    [Header("Destruction Settings (Toggle Hilang/Hancur)")]
    [Tooltip("Jika dicentang, proyektil akan hancur saat mengenai Player")]
    public bool destroyOnPlayerHit = true; 
    
    [Tooltip("Jika dicentang, proyektil akan hancur saat mengenai Lantai atau Dinding/Environment")]
    public bool destroyOnEnvironmentHit = true; 

    private Vector3 moveDirection;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetupTarget(Transform playerTransform)
    {
        Vector3 targetCenter = new Vector3(playerTransform.position.x, playerTransform.position.y + 1f, playerTransform.position.z);
        moveDirection = (targetCenter - transform.position).normalized;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Deteksi Tabrakan dengan Player
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            if (destroyOnPlayerHit)
            {
                Destroy(gameObject);
            }
        }
        // 2. Deteksi Tabrakan dengan Lingkungan / Lantai
        else if (other.CompareTag("Environment") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (destroyOnEnvironmentHit)
            {
                Destroy(gameObject);
            }
        }
    }
}