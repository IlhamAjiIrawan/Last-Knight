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

    public void RetryGame()
    {
        Time.timeScale = 1f; 

        // ------------------------------------------------------------------
        // PERUBAHAN UTAMA:
        // Ganti ResetStats() menjadi LoadStats() agar darah yang 0 HP dan 
        // status mati di-overwrite kembali oleh data terakhir saat berhasil clear wave.
        // ------------------------------------------------------------------
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.LoadStats();
            Debug.Log("<color=yellow>GAME OVER SYSTEM: Mengembalikan statistik player ke Checkpoint terakhir.</color>");
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f; 

        // Saat kembali ke main menu, kita juga panggil LoadStats() agar data runtime 
        // tidak dalam kondisi 'mati/0 HP'. Jadi jika player menekan Play lagi di Main Menu, 
        // mereka otomatis melanjutkan dari save data wave terakhir mereka.
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.LoadStats();
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