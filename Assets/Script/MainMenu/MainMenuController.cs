using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

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

    private string saveFileName = "save_player_data.json";

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

        SceneManager.LoadScene("Map1Village");
    }

    // ======================================================================
    // FUNGSI LOAD GAME DENGAN PEMBACAAN DINAMIS & AMAN
    // ======================================================================
    public void LoadGame()
    {
        string savePath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (File.Exists(savePath))
        {
            Debug.Log("[Main Menu]: File data ditemukan. Memuat progres pemain...");
            
            // Fallback default jika file JSON korup
            string sceneToLoad = "Map1Village"; 

            try
            {
                // Baca string JSON secara langsung dari local storage
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
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("[Main Menu]: Tidak bisa Load Game karena tidak ada file simpanan lama!");
        }
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