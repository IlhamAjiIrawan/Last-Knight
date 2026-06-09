using UnityEngine;
using UnityEngine.SceneManagement; // PENTING: Untuk mengatur perpindahan scene

public class GameOverManager : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject gameOverPanel;

    [Header("Scene Settings")]
    [Tooltip("Tuliskan nama scene Main Menu kamu dengan tepat")]
    public string mainMenuSceneName = "MainMenu"; 

    private GameObject player;

    void Start()
    {
        // Pastikan panel Game Over tertutup saat game baru dimulai
        if (gameOverPanel != null) 
            gameOverPanel.SetActive(false);

        // Cari Player di dalam scene berdasarkan Tag
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                // Berlangganan ke event onDeath milik Player
                playerHealth.onDeath += TriggerGameOver;
            }
        }
        else
        {
            Debug.LogError("[GameOverManager]: Objek dengan tag 'Player' tidak ditemukan di Scene!");
        }
    }

    // Fungsi yang otomatis dipanggil saat player mati
    void TriggerGameOver()
    {
        Debug.LogWarning("PLAYER DIKEDALKAN! Membuka Panel Game Over...");
        
        if (gameOverPanel != null) 
            gameOverPanel.SetActive(true);

        // Hentikan waktu game dan munculkan kursor mouse untuk klik UI
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- FUNGSI UNTUK TOMBOL RETRY ---
    public void RetryGame()
    {
        Time.timeScale = 1f; 

        // Panggil fungsi reset dari PlayerStats sebelum memuat ulang level
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.ResetStats();
            // Catatan: Jika ingin meriset Gold dan Potion juga, 
            // ganti menjadi: PlayerStats.instance.ResetAllProgression();
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    // --- FUNGSI UNTUK TOMBOL MAIN MENU ---
    public void BackToMainMenu()
    {
        Time.timeScale = 1f; 

        // Pastikan saat kembali ke main menu, statistik player sudah bersih saat bermain lagi nanti
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.ResetStats();
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        // Unsubscribe dari event saat objek dihancurkan untuk menghindari memory leak
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.onDeath -= TriggerGameOver;
            }
        }
    }
}