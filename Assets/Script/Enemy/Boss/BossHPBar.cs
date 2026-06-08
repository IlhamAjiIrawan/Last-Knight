using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHPBar : MonoBehaviour
{
    public Slider healthSlider;
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI hpText;

    private Health targetBossHealth;

    // Fungsi inisialisasi awal saat UI ini dipasangkan ke Boss tertentu
    public void Setup(Health bossHealth, string bossName)
    {
        targetBossHealth = bossHealth;
        
        if (bossNameText != null) 
            bossNameText.text = bossName;
            
        if (healthSlider != null && targetBossHealth != null)
        {
            healthSlider.maxValue = targetBossHealth.maxHealth;
            healthSlider.value = targetBossHealth.currentHealth;
        }
    }

    void Update()
    {
        // Jika Boss masih hidup, update data darahnya secara real-time
        if (targetBossHealth != null)
        {
            if (healthSlider != null)
            {
                healthSlider.value = targetBossHealth.currentHealth;
            }
            if (hpText != null)
            {
                hpText.text = Mathf.Max(0, (int)targetBossHealth.currentHealth) + " / " + (int)targetBossHealth.maxHealth;
            }
        }
        else
        {
            // Jika objek Boss sudah hancur/mati, hapus UI Bar ini dari layar
            Destroy(gameObject);
        }
    }
}