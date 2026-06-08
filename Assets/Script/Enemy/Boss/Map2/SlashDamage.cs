using UnityEngine;

public class SlashDamage : MonoBehaviour
{
    public float damage = 30f;
    private bool hasDamagedPlayer = false; 

    private void OnTriggerEnter(Collider other)
    {
        // 1. Cek apakah objek yang ditabrak adalah Player dan belum pernah diberi damage oleh objek ini
        if (other.CompareTag("Player") && !hasDamagedPlayer)
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                // 2. Berikan damage ke Player
                playerHealth.TakeDamage(damage);
                
                // 3. Kunci agar tidak memberikan damage lagi saat peluru sedang berjalan menembus tubuh player
                hasDamagedPlayer = true; 
                
                Debug.Log("<color=red>💥 Player terkena Straight Slash (Menembus)! Damage: </color>" + damage);
            }
        }
    }
}