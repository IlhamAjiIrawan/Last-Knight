using UnityEngine;
using System.Collections.Generic; // PENTING: Untuk mengaktifkan fitur List

public class HorizontalSlash : MonoBehaviour
{
    public float flySpeed = 15f;        // Kecepatan terbang tebasan
    public float maxLifetime = 3f;      // Waktu hancur otomatis jika tidak mengenai apa-apa
    private float slashDamage;
    private Rigidbody rb;

    // MENGIKUTI REFERENSI: Mencegah satu musuh terkena hit berkali-kali saat tebasan lewat
    private List<Health> hitEnemies = new List<Health>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    // MENGIKUTI REFERENSI: Fungsi inisialisasi arah, kecepatan, dan besar damage
    public void SetupSlash(float damage)
    {
        slashDamage = damage;

        if (rb != null)
        {
            // Menggunakan linearVelocity sesuai dengan Unity versi baru pada ArrowProjectile
            rb.linearVelocity = transform.forward * flySpeed;
        }
        else
        {
            Debug.LogWarning("Peringatan: Rigidbody belum dipasang pada Prefab Horizontal Slash!");
        }

        Destroy(gameObject, maxLifetime); 
    }

    void OnTriggerEnter(Collider other)
    {
        // MENGIKUTI REFERENSI: Jika mengenai objek dengan tag "Enemy"
        if (other.CompareTag("Enemy"))
        {
            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                // Cek apakah musuh ini sudah pernah terkena tebasan ini atau belum
                if (!hitEnemies.Contains(enemyHealth))
                {
                    enemyHealth.TakeDamage(slashDamage); // Berikan damage
                    hitEnemies.Add(enemyHealth);         // Kunci musuh ini ke dalam daftar hitam tebasan ini
                    Debug.Log("Tebasan horizontal menembus musuh: " + other.name + " | Damage: " + slashDamage);
                }
            }

            // PERBEDAAN: JANGAN panggil Destroy(gameObject) di sini agar proyektil tetap melaju menembus musuh lain!
        }
        // MENGIKUTI REFERENSI: Hancur jika menabrak tanah atau dinding pembatas map
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}