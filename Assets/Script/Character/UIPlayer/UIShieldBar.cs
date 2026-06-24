using UnityEngine;
using UnityEngine.UI;

public class UIShieldBar : MonoBehaviour
{
    public Slider shieldSlider;
    public PlayerMovement playerMovement;
    
    // --- TAMBAHAN: Variabel Fill Area untuk mengatur visual gambar Slider ---
    public Image fillArea; 

    [Header("Color Settings")]
    public Color shieldColor = new Color(1f, 0.84f, 0f); // Warna kuning emas default untuk Shield

    private CanvasGroup canvasGroup;

    void Start()
    {
        // Menambahkan CanvasGroup secara otomatis jika belum ada di objek ini
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Atur warna fill area di awal permainan jika komponennya sudah dimasukkan
        if (fillArea != null)
        {
            fillArea.color = shieldColor;
        }

        // Sembunyikan di awal permainan karena shield belum aktif
        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (playerMovement == null || shieldSlider == null) return;

        // 1. Munculkan dan update bar hanya jika shield sedang aktif
        if (playerMovement.isShieldActive && playerMovement.currentShieldHp > 0)
        {
            canvasGroup.alpha = 1f; // Memunculkan bar secara visual
            
            // PENTING: Selalu sinkronkan Max Value secara dinamis agar Slider tidak nge-bug (bernilai 0)
            shieldSlider.maxValue = playerMovement.maxShieldHp;
            shieldSlider.value = playerMovement.currentShieldHp;
        }
        else
        {
            canvasGroup.alpha = 0f; // Sembunyikan bar saat shield habis atau mati
        }

        // 2. Membuat UI selalu menghadap ke arah Kamera (Billboard Effect)
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}