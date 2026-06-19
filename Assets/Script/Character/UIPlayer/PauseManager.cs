using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // <-- TAMBAHKAN INI untuk fitur pindah Scene (Main Menu)

public class PauseManager : MonoBehaviour
{
    [Header("UI Panel Pause & Sub-Panels")]
    public GameObject pausePanel;    // Tarik Game Object Panel Pop-Up Pause ke sini
    public GameObject settingsPanel; // Tarik Game Object Panel Setting ke sini
    private bool isPaused = false;

    [Header("UI Teks Statistik Player")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI mpText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI goldText;

    void Start()
    {
        // Pastikan semua panel tertutup saat game dimulai
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        Time.timeScale = 1f; // Kembalikan waktu normal
    }

    void Update()
    {
        // Mendeteksi tombol Escape (ESC) dipencet
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // Jika panel setting lagi terbuka, tutup setting dulu dan kembali ke panel pause utama
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    CloseSettings();
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        Time.timeScale = 0f; // Bekukan AI & Waktu game
        UpdateStatsUI();
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        Time.timeScale = 1f; // Jalankan kembali AI & Waktu game
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            if (pausePanel != null) pausePanel.SetActive(false); // Sembunyikan panel pause utama agar tidak menumpuk
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true); // Munculkan kembali panel pause utama
    }

    // Masukkan nama scene Menu Utama Anda di kolom Inspector tombol nantinya
    public void ReturnToMainMenu(string sceneNameMenuUtama)
    {
        Time.timeScale = 1f; // PENTING: Wajib disetel ke 1f sebelum pindah scene, jika tidak menu utama akan ikut membeku!
        SceneManager.LoadScene(sceneNameMenuUtama);
    }

    void UpdateStatsUI()
    {
        if (PlayerStats.instance == null) return;

        if (healthText) healthText.text = $"HP: {Mathf.RoundToInt(PlayerStats.instance.currentHealth)} / {PlayerStats.instance.maxHealth}";
        if (mpText) mpText.text = $"MP: {Mathf.RoundToInt(PlayerStats.instance.currentMP)} / {PlayerStats.instance.maxMP}";
        if (energyText) energyText.text = $"Energy: {Mathf.RoundToInt(PlayerStats.instance.currentEnergy)} / {PlayerStats.instance.maxEnergy}";
        if (damageText) damageText.text = $"Damage: {PlayerStats.instance.damage}";
        if (speedText) speedText.text = $"Speed: {PlayerStats.instance.speed}";
        if (goldText) goldText.text = $"Gold: {PlayerStats.instance.gold}";
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Keluar dari game...");
    }
}