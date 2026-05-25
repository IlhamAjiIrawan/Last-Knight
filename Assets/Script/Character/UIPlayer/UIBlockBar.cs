using UnityEngine;
using UnityEngine.UI;

public class UIBlockBar : MonoBehaviour
{
    public Slider blockSlider;
    public PlayerMovement playerMovement; // Referensi ke script PlayerMovement
    public GameObject fillArea;           // Referensi ke visual utama slider (opsional, untuk menyembunyikan bar)

    private CanvasGroup canvasGroup;       // Digunakan untuk menghilangkan/memunculkan UI dengan halus

    void Start()
    {
        // Menambahkan CanvasGroup secara otomatis jika belum ada di objek ini
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (playerMovement != null && blockSlider != null)
        {
            blockSlider.maxValue = playerMovement.maxBlockGauge;
        }

        // Sembunyikan di awal permainan karena gauge masih penuh
        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (playerMovement == null || blockSlider == null) return;

        // 1. Selalu update nilai slider mengikuti sisa pertahanan player
        blockSlider.value = playerMovement.currentBlockGauge;

        // 2. Logika Muncul/Hilang otomatis
        // Bar akan MUNCUL jika player sedang blocking ATAU stamina tamengnya belum pulih penuh (< 100%)
        if (playerMovement.isBlocking || playerMovement.currentBlockGauge < playerMovement.maxBlockGauge)
        {
            canvasGroup.alpha = 1f; // Memunculkan bar
        }
        else
        {
            canvasGroup.alpha = 0f; // Menyembunyikan bar saat sudah pulih total (100%)
        }

        // 3. TAMBAHAN: Membuat UI selalu menghadap ke arah Kamera (Anti-Terbalik saat Karakter Berputar)
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}