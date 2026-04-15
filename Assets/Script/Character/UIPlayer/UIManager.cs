using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider healthSlider;
    public Slider mpSlider;

    [Header("UI Texts")]
    public TextMeshProUGUI healthText; // Tipe data khusus untuk TMP di UI
    public TextMeshProUGUI mpText;

    void Start()
    {
        if (PlayerStats.instance != null)
        {
            healthSlider.maxValue = PlayerStats.instance.maxHealth;
            mpSlider.maxValue = PlayerStats.instance.maxMP;
        }
    }

    void Update()
    {
        if (PlayerStats.instance == null) return;

        // 1. Update Slider
        healthSlider.value = PlayerStats.instance.currentHealth;
        mpSlider.value = PlayerStats.instance.currentMP;

        // 2. Update Text (Format: "Current / Max")
        // Mathf.FloorToInt digunakan agar angka desimal (seperti 75.5) terlihat bulat menjadi 75
        healthText.text = Mathf.FloorToInt(PlayerStats.instance.currentHealth) + " / " + PlayerStats.instance.maxHealth;
        
        mpText.text = Mathf.FloorToInt(PlayerStats.instance.currentMP) + " / " + PlayerStats.instance.maxMP;
    }
}