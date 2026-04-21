using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider healthSlider;
    public Slider mpSlider;
    public Slider energySlider;

    [Header("UI Texts")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI mpText;
    public TextMeshProUGUI energyText;

    void Start()
    {
        if (PlayerStats.instance != null)
        {
            healthSlider.maxValue = PlayerStats.instance.maxHealth;
            mpSlider.maxValue = PlayerStats.instance.maxMP;
            energySlider.maxValue = PlayerStats.instance.maxEnergy;
        }
    }

    void Update()
    {
        if (PlayerStats.instance == null) return;

        // 1. Update Slider
        healthSlider.value = PlayerStats.instance.currentHealth;
        mpSlider.value = PlayerStats.instance.currentMP;
        energySlider.value = PlayerStats.instance.currentEnergy;

        // 2. Update Text (Format: "Current / Max")
        // Mathf.FloorToInt digunakan agar angka desimal (seperti 75.5) terlihat bulat menjadi 75
        healthText.text = Mathf.FloorToInt(PlayerStats.instance.currentHealth) + " / " + PlayerStats.instance.maxHealth;
        mpText.text = Mathf.FloorToInt(PlayerStats.instance.currentMP) + " / " + PlayerStats.instance.maxMP;
        energyText.text = Mathf.FloorToInt(PlayerStats.instance.currentEnergy) + " / " + PlayerStats.instance.maxEnergy;
    }
}