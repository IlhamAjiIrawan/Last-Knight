using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Singleton instance
    public static PlayerStats instance;

    [Header("Status yang Disimpan")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    public float damage = 20f;
    public float speed = 5.0f;

    void Awake()
    {
        // Logika Singleton: Pastikan hanya ada SATU objek ini di seluruh game
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // JANGAN hapus saat pindah Scene
        }
        else
        {
            Destroy(gameObject); // Hapus jika ada duplikat di Scene baru
        }
    }
}