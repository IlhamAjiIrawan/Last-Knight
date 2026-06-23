using UnityEngine;
using UnityEngine.Video; // WAJIB: Untuk mengontrol pemutar video
using UnityEngine.SceneManagement; // WAJIB: Untuk perpindahan scene

public class StorySceneController : MonoBehaviour
{
    [Header("Komponen Video")]
    public VideoPlayer videoPlayer;

    [Header("UI Pop-up Settings")]
    [Tooltip("Tarik GameObject Panel Pop-up / Pause kamu ke sini")]
    public GameObject continuePanel; 
    
    [Tooltip("Tarik GameObject Tombol RESUME/KEMBALI yang ada di dalam panel ke sini")]
    public GameObject resumeButton;

    [Header("Scene Destination")]
    [Tooltip("Tuliskan nama scene tujuan setelah tombol Lanjut diklik")]
    public string nextSceneName;

    // Variabel internal untuk mengecek status video
    private bool isVideoFinished = false;

    void Start()
    {
        if (continuePanel != null)
        {
            continuePanel.SetActive(false);
        }

        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            // Berlangganan ke event saat video selesai secara alami
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void Update()
    {
        // Fitur Pause lewat ESC hanya bekerja jika video BELUM selesai
        if (Input.GetKeyDown(KeyCode.Escape) && !isVideoFinished)
        {
            if (continuePanel != null)
            {
                if (continuePanel.activeSelf)
                {
                    ClosePopupAndResume();
                }
                else if (videoPlayer != null && videoPlayer.isPlaying)
                {
                    PauseVideoAndOpenPopup();
                }
            }
        }
    }

    // CONDITION 1: Jika di-pause via ESC saat video belum selesai
    void PauseVideoAndOpenPopup()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause(); 
        }
        
        // Tampilkan tombol resume karena player bisa kembali menonton
        if (resumeButton != null) 
        {
            resumeButton.SetActive(true); 
        }

        TriggerPopup(true);
    }

    // Fungsi untuk menutup pop-up dan melanjutkan video kembali
    public void ClosePopupAndResume()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play(); 
        }

        TriggerPopup(false);
    }

    // CONDITION 2: Otomatis berjalan saat video .mp4 HABIS secara alami
    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[StoryScene]: Video habis! Memunculkan pop-up untuk pindah scene.");
        isVideoFinished = true; // Tandai video sudah tamat
        
        // SEMBUNYIKAN tombol resume (karena video sudah habis, tidak bisa di-resume lagi)
        if (resumeButton != null) 
        {
            resumeButton.SetActive(false); 
        }

        TriggerPopup(true);
    }

    // Fungsi pembantu untuk mengaktifkan UI Pop-up
    void TriggerPopup(bool show)
    {
        if (continuePanel != null)
        {
            continuePanel.SetActive(show);
        }

        if (show)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ContinueToNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // FITUR BARU: Simpan progress status + daftarkan scene tujuan sebagai checkpoint terakhir
            if (PlayerStats.instance != null)
            {
                PlayerStats.instance.SaveStats(nextSceneName);
            }

            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("[StoryScene Error]: Kamu belum mengisi nama 'Next Scene Name' di Inspector!");
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}