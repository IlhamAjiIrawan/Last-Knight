using UnityEngine;
using System.Collections.Generic;

public class ArrowProjectile : MonoBehaviour
{
    public float flySpeed = 22f;
    public float maxLifetime = 3f;
    private float arrowDamage;
    private Rigidbody rb;

    private bool isChargedShot = false;
    private List<Health> hitEnemies = new List<Health>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public void SetupProjectile(float damage, bool isCharged)
    {
        arrowDamage = damage;
        isChargedShot = isCharged;

        if (isCharged)
        {
            transform.localScale *= 1.6f; 
        }

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * flySpeed;
        }
        else
        {
            Debug.LogWarning("Peringatan: Rigidbody belum dipasang pada Prefab Anak Panah!");
        }

        Destroy(gameObject, maxLifetime); 
    }

    void OnTriggerEnter(Collider other)
    {
        // Jika mengenai objek yang memiliki Tag "Enemy"
        if (other.CompareTag("Enemy"))
        {
            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                if (!hitEnemies.Contains(enemyHealth))
                {
                    enemyHealth.TakeDamage(arrowDamage); // Berikan damage
                    hitEnemies.Add(enemyHealth);         // Kunci musuh ini ke dalam daftar hitam
                    Debug.Log("Panah menembus musuh: " + other.name + " | Damage: " + arrowDamage);
                }
            }

            if (!isChargedShot)
            {
                Destroy(gameObject);
            }
        }

        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (!isChargedShot)
            {
                Destroy(gameObject);
            }
        }
    }
}