using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider healthSlider;
    public Slider mpSlider;
    public Slider energySlider;
    public Slider rageSlider;

    [Header("UI Texts")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI mpText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI rageText;

/*
    private void Start()
    {
        UpdateMaxValues();
    }
*/

    void Update()
    {
        if (PlayerStats.instance == null) return;

        // 1. Update Slider
        healthSlider.value = PlayerStats.instance.currentHealth;
        mpSlider.value = PlayerStats.instance.currentMP;
        energySlider.value = PlayerStats.instance.currentEnergy;

        // 2. Update Text (Format: "Current / Max")
        healthText.text = Mathf.FloorToInt(PlayerStats.instance.currentHealth) + " / " + PlayerStats.instance.maxHealth;
        mpText.text = Mathf.FloorToInt(PlayerStats.instance.currentMP) + " / " + PlayerStats.instance.maxMP;
        energyText.text = Mathf.FloorToInt(PlayerStats.instance.currentEnergy) + " / " + PlayerStats.instance.maxEnergy;

        healthSlider.maxValue = PlayerStats.instance.maxHealth;
        mpSlider.maxValue = PlayerStats.instance.maxMP;
        energySlider.maxValue = PlayerStats.instance.maxEnergy;

        // 3. Update Gold Text
        if (goldText != null)
        {
            goldText.text = " " + PlayerStats.instance.gold.ToString();
        }

        if (rageSlider != null)
        {
            rageSlider.maxValue = PlayerStats.instance.maxRage;
            rageSlider.value = PlayerStats.instance.currentRage;
        }

        if (rageText != null)
        {
            // Jika sudah penuh, tampilkan pesan khusus
            if (PlayerStats.instance.currentRage >= PlayerStats.instance.maxRage)
            {
                rageText.text = "READY! (Press Q)";
                rageText.color = Color.red; // Opsional: Ubah warna jadi merah saat penuh
            }
            else
            {
                // Tampilkan angka bulat "0 / 100"
                rageText.text = Mathf.FloorToInt(PlayerStats.instance.currentRage) + " / " + PlayerStats.instance.maxRage;
                // Mengubah warna teks menggunakan kode Hex #E0C565
                if (ColorUtility.TryParseHtmlString("#E0C565", out Color customColor))
                {
                    rageText.color = customColor;
                }
            }
        }
    }
}