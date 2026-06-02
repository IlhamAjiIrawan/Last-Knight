using UnityEngine;
using UnityEngine.UI;

public class UIChargeBar : MonoBehaviour
{
    public Slider chargeSlider;
    public PlayerMovement playerMovement;
    public Image fillArea;

    [Header("Color Settings")]
    public Color chargingColor = Color.yellow;
    public Color maxDamageColor = Color.red;
    
    private CanvasGroup canvasGroup;

    void Start()
    {
        // Menambahkan CanvasGroup secara otomatis jika belum ada di objek ini
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (chargeSlider != null)
        {
            chargeSlider.minValue = 0f;
            chargeSlider.maxValue = 2f; // Sesuai dengan batas maksimum chargeTimer di PlayerMovement (2 detik)
        }

        // Sembunyikan di awal permainan karena player belum membidik
        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (playerMovement == null || chargeSlider == null) return;

        // 1. Logika Muncul/Hilang otomatis & Update Nilai Slider
        // Bar hanya muncul jika masuk mode panah DAN sedang menahan klik kanan (isCharging)
        if (playerMovement.isBowMode && playerMovement.isCharging)
        {
            canvasGroup.alpha = 1f; // Memunculkan bar
            chargeSlider.value = playerMovement.chargeTimer; // Update nilai slider sesuai timer panah
            
            // 2. Logika Perubahan Warna Dinamis
            if (fillArea != null)
            {
                // Jika charge sudah maksimal (2 detik), ubah warna menjadi warna Max Damage
                if (playerMovement.chargeTimer >= 2f)
                {
                    fillArea.color = maxDamageColor;
                }
                else
                {
                    fillArea.color = chargingColor;
                }
            }
        }
        else
        {
            canvasGroup.alpha = 0f; // Sembunyikan bar jika sedang tidak membidik atau panah sudah lepas
            chargeSlider.value = 0f; // Reset nilai slider ke nol
        }

        // 3. Membuat UI selalu menghadap ke arah Kamera (Billboarding agar anti-terbalik)
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}