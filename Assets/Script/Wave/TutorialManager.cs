using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    // Struktur tahapan tutorial
    public enum TutorialState { Gerak, Menghindar, Menyerang, GunakanPotion, Selesai }
    
    [Header("Status Tutorial Saat Ini")]
    public TutorialState currentState = TutorialState.Gerak;

    [Header("UI Teks Petunjuk")]
    public TextMeshProUGUI instructionText;

    [Header("Pengaturan Pertarungan")]
    public GameObject dummyEnemyPrefab;
    public Transform enemySpawnPoint;

    [Header("Pengaturan Audio BGM (Baru)")]
    [Tooltip("Tarik komponen AudioSource dari scene ke sini")]
    public AudioSource bgmSource;
    [Tooltip("Musik latar santai untuk latihan dasar (Gerak, Menghindar, Potion)")]
    public AudioClip tutorialNormalBGM;
    [Tooltip("Musik pertempuran saat musuh dummy muncul")]
    public AudioClip tutorialCombatBGM;

    [Header("Perpindahan Scene Utama")]
    public string nextSceneName = "Map1Village";

    private bool isStateChanging = false;
    private GameObject spawnedEnemy;

    void Start()
    {
        // Berikan pemain 1 buah Potion di awal untuk bahan uji coba tutorial
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.smallPotionCount = 1;
        }

        // Jalankan BGM awal saat tutorial dimulai
        PlayBGM(tutorialNormalBGM);

        // Jalankan tahap pertama
        SetTutorialState(TutorialState.Gerak);
    }

    void Update()
    {
        // Logika pengecekan input pemain di setiap tahapan
        switch (currentState)
        {
            case TutorialState.Gerak:
                if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                {
                    AdvanceStateAfterDelay(TutorialState.Menghindar, 2f);
                }
                break;

            case TutorialState.Menghindar:
                if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space))
                {
                    AdvanceStateAfterDelay(TutorialState.Menyerang, 1.5f);
                }
                break;

            case TutorialState.Menyerang:
                // Selesai otomatis saat musuh dummy mati (diatur lewat Event OnDummyEnemyDefeated)
                break;

            case TutorialState.GunakanPotion:
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1) ||
                    Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                {
                    AdvanceStateAfterDelay(TutorialState.Selesai, 1.5f);
                }
                break;
        }
    }

    // Fungsi untuk mengganti text, mempersiapkan aksi, dan mengubah musik di setiap tahap
    void SetTutorialState(TutorialState newState)
    {
        currentState = newState;
        isStateChanging = false;

        switch (currentState)
        {
            case TutorialState.Gerak:
                instructionText.text = "Gunakan tombol [W, A, S, D] atau [Tombol Arah] untuk bergerak.";
                break;

            case TutorialState.Menghindar:
                instructionText.text = "Bagus! Sekarang, tekan [Left Shift] atau [Space] untuk MENGHINDAR (Dash).";
                break;

            case TutorialState.Menyerang:
                instructionText.text = "Bahaya! Musuh muncul.\nDekati musuh dan gunakan [Klik Kiri] untuk MENYERANG.";
                
                // GANTI MUSIK: Set ke musik bertarung saat musuh muncul
                PlayBGM(tutorialCombatBGM);
                
                SpawnDummyEnemy();
                break;

            case TutorialState.GunakanPotion:
                instructionText.text = "Kerja bagus! Darah atau Mana kamu berkurang.\nTekan [Tombol 1] untuk MENGGUNAKAN POTION.";
                
                // GANTI MUSIK: Kembalikan ke musik santai setelah musuh kalah
                PlayBGM(tutorialNormalBGM);
                break;

            case TutorialState.Selesai:
                instructionText.text = "Tutorial Selesai! Kamu siap bertualang.\nMemasuki Desa dalam 3 detik...";
                
                // Efek musik mengecil (Fade Out) saat tutorial selesai
                StartCoroutine(FadeOutBGM(2.5f));
                
                StartCoroutine(FinishTutorialRoutine());
                break;
        }
    }

    // Fungsi pembantu untuk memutar audio musik
    void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        
        // Jika lagu yang ingin diputar sudah sama dan sedang berjalan, abaikan agar tidak mengulang dari awal
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = 0.5f; // Mengatur volume default (0.0f sampai 1.0f)
        bgmSource.Play();
    }

    // Coroutine untuk membuat volume musik mengecil perlahan saat scene akan berganti
    IEnumerator FadeOutBGM(float duration)
    {
        if (bgmSource == null) yield break;

        float startVolume = bgmSource.volume;

        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        bgmSource.Stop();
    }

    void AdvanceStateAfterDelay(TutorialState nextState, float delay)
    {
        if (isStateChanging) return;
        isStateChanging = true;
        StartCoroutine(ChangeStateRoutine(nextState, delay));
    }

    IEnumerator ChangeStateRoutine(TutorialState nextState, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetTutorialState(nextState);
    }

    void SpawnDummyEnemy()
    {
        if (dummyEnemyPrefab != null && enemySpawnPoint != null)
        {
            spawnedEnemy = Instantiate(dummyEnemyPrefab, enemySpawnPoint.position, Quaternion.identity);

            Health enemyHealth = spawnedEnemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.onDeath += OnDummyEnemyDefeated;
            }
        }
        else
        {
            Debug.LogError("TutorialManager: Prefab Musuh atau Spawn Point kosong!");
            SetTutorialState(TutorialState.GunakanPotion);
        }
    }

    void OnDummyEnemyDefeated()
    {
        if (currentState == TutorialState.Menyerang)
        {
            SetTutorialState(TutorialState.GunakanPotion);
        }
    }

    IEnumerator FinishTutorialRoutine()
    {
        yield return new WaitForSeconds(3f);

        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.SaveStats(nextSceneName);
        }

        SceneManager.LoadScene(nextSceneName);
    }
}