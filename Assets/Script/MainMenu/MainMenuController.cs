using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections; // WAJIB: Ditambahkan untuk mendukung Coroutine Fade Out BGM

// Blueprint pembantu diletakkan di luar class agar aman dari error scope kompilasi (CS0246)
[System.Serializable]
public class SceneBlueprintData
{
    public string lastSavedScene;
}

public class MainMenuController : MonoBehaviour
{
    [Header("Panel Pop-up UI")]
    public GameObject settingPanel;
    public GameObject exitConfirmationPanel;

    [Header("Audio Settings (Baru)")]
    [Tooltip("Tarik komponen AudioSource dari scene Main Menu ke sini")]
    public AudioSource bgmSource;
    [Tooltip("Tarik file audio/musik untuk Main Menu ke sini")]
    public AudioClip mainMenuBGM;

    private string saveFileName = "save_player_data.json";

    void Start()
    {
        // Jalankan musik saat Main Menu pertama kali terbuka
        PlayBGM();
    }

    void PlayBGM()
    {
        if (bgmSource != null && mainMenuBGM != null)
        {
            bgmSource.clip = mainMenuBGM;
            bgmSource.loop = true;
            bgmSource.volume = 0.5f; // Mengatur volume default awal (50%)
            bgmSource.Play();
        }
    }

    public void NewGame()
    {
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.ResetStats();
        }
        else
        {
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("[Main Menu]: File save lama berhasil DIHAPUS untuk Game Baru.");
            }
        }

        // Mulai game menggunakan Coroutine agar musik mengecil perlahan sebelum pindah scene
        StartCoroutine(LoadSceneWithFadeOut("SampleScene"));
    }

    public void LoadGame()
    {
        string savePath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (File.Exists(savePath))
        {
            Debug.Log("[Main Menu]: File data ditemukan. Memuat progres pemain...");
            
            string sceneToLoad = "Map1Village"; // Fallback default jika file JSON korup
            try
            {
                string jsonText = File.ReadAllText(savePath);
                SceneBlueprintData data = JsonUtility.FromJson<SceneBlueprintData>(jsonText);

                if (data != null && !string.IsNullOrEmpty(data.lastSavedScene))
                {
                    sceneToLoad = data.lastSavedScene;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Main Menu Error]: Gagal membaca file JSON: " + e.Message);
            }

            Debug.Log("[Main Menu]: Berhasil mendeteksi lokasi terakhir! Berpindah ke: " + sceneToLoad);
            
            // Pindah ke scene simpanan terakhir dengan efek musik fade out
            StartCoroutine(LoadSceneWithFadeOut(sceneToLoad));
        }
        else
        {
            Debug.LogWarning("[Main Menu]: Tidak bisa Load Game karena tidak ada file simpanan lama!");
        }
    }

    // Coroutine pembantu untuk mengecilkan volume musik secara halus sebelum scene berganti
    IEnumerator LoadSceneWithFadeOut(string sceneName)
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            float duration = 1.0f; // Durasi transisi audio mengecil (1 detik)

            while (bgmSource.volume > 0)
            {
                bgmSource.volume -= startVolume * Time.deltaTime / duration;
                yield return null;
            }
            bgmSource.Stop();
        }

        // Setelah musik benar-benar mati/mengecil, baru lakukan perpindahan scene
        SceneManager.LoadScene(sceneName);
    }

    public void OpenSetting()
     {
         if (settingPanel != null) settingPanel.SetActive(true);
     }

    public void CloseSetting()
     {
         if (settingPanel != null) settingPanel.SetActive(false);
     }

    public void OpenExitConfirmation()
    {
        if (exitConfirmationPanel != null) exitConfirmationPanel.SetActive(true);
    }

    public void CloseExitConfirmation()
    {
        if (exitConfirmationPanel != null) exitConfirmationPanel.SetActive(false);
    }

    public void ConfirmExitGame()
    {
        Debug.Log("Menutup Aplikasi Game...");
        Application.Quit();
    }
}