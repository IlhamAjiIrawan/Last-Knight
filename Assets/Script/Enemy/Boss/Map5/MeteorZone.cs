using UnityEngine;
using System.Collections;

public class MeteorZone : MonoBehaviour
{
    private float radius;
    private float damagePerTick;
    private float duration;
    private float tickInterval;

    // Fungsi inisialisasi yang dipanggil saat bos memunculkan prefab ini
    public void SetupMeteor(float radius, float damage, float duration, float interval)
    {
        this.radius = radius;
        this.damagePerTick = damage;
        this.duration = duration;
        this.tickInterval = interval;

        // Mulai siklus damage dan penghancuran mandiri
        StartCoroutine(MeteorLifetimeRoutine());
    }

    IEnumerator MeteorLifetimeRoutine()
    {
        float elapsed = 0f;

        // Loop DoT berjalan mandiri selama durasi yang ditentukan oleh bos
        while (elapsed < duration)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider col in hits)
            {
                if (col.CompareTag("Player"))
                {
                    Health playerHealth = col.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damagePerTick);
                        Debug.Log("<color=red>☄️ Player terkena tick damage Hujan Meteor! Damage: </color>" + damagePerTick);
                    }
                }
            }

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        // ========== AGAR METEOR MENGHILANG DENGAN HALUS ==========
        // Menghentikan emisi partikel baru agar meteor tidak hilang secara patah/kaget
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            var emission = ps.emission;
            emission.enabled = false; 
        }

        // Berikan jeda waktu (misal 2 detik) agar sisa meteor yang terlanjur meluncur selesai jatuh ke tanah
        yield return new WaitForSeconds(2.0f);

        // Hancurkan objek secara total dari scene
        Destroy(gameObject);
    }

    // Opsional: Untuk melihat radius area meteor di Scene View saat didebug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}