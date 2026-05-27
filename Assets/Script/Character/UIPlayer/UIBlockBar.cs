using UnityEngine;
using UnityEngine.UI;

public class UIBlockBar : MonoBehaviour
{
    public Slider blockSlider;
    public PlayerMovement playerMovement;
    public Image fillArea;

    [Header("Color Settings")]
    public Color normalColor = Color.cyan;
    public Color brokenColor = Color.red;
    private CanvasGroup canvasGroup;

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

        if (fillArea != null)
        {
            if (playerMovement.isBlockBroken)
            {
                fillArea.color = brokenColor; // Berubah jadi merah jika tameng hancur
            }
            else
            {
                fillArea.color = normalColor; // Kembali ke warna normal jika sudah aman
            }
        }

        // 2. Logika Muncul/Hilang otomatis
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