using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class WaveManager : MonoBehaviour {
    public List<WaveData> allWaves;
    public Transform[] spawnPoints;
    public GameObject shopPanel;  

    [Header("Boss Wave Settings")]
    public GameObject bossPrefab;      // Masukkan Prefab Boss (yang memakai script BossAI) di sini
    public Transform bossSpawnPoint;
    public GameObject bossHPBarUI;
    public TextMeshProUGUI textBarBoss;
    private int currentWaveIndex = 0;
    private int enemiesRemaining = 0;
    
    // VARIABEL BARU: Untuk menyimpan data komponen Health dari Boss yang sedang aktif
    private Health activeBossHealth;

    void Start() {
        StartCoroutine(StartWave());
    }

    // MEKANISME BARU: Update teks HP secara real-time dari data Boss yang aktif
    void Update() {
        if (activeBossHealth != null && textBarBoss != null) {
            // Mengambil currentHealth dan maxHealth langsung dari komponen Health si Boss
            textBarBoss.text = Mathf.Max(0, (int)activeBossHealth.currentHealth) + " / " + (int)activeBossHealth.maxHealth;
        }
    }

    IEnumerator StartWave() {
        if (currentWaveIndex >= allWaves.Count) {
            Debug.Log("Semua Wave Selesai!");
            yield break;
        }

        WaveData wave = allWaves[currentWaveIndex];

        foreach (var group in wave.enemiesInWave) {
            for (int i = 0; i < group.count; i++) {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        if (wave.isBossWave) {
            Debug.LogWarning("PERINGATAN: WAVE BOSS TELAH DIMULAI!");
            SpawnBoss();
        }
    }

    void SpawnEnemy(GameObject prefab) {
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(prefab, randomPoint.position, Quaternion.identity);
        
        // Hubungkan sinyal mati musuh ke fungsi di Manager ini
        enemy.GetComponent<Health>().onDeath += OnEnemyDefeated;
        enemiesRemaining++;
    }

    void SpawnBoss() {
        if (bossPrefab == null) {
            Debug.LogError("Gagal Spawn! Prefab Boss belum dimasukkan ke WaveManager di Inspector.");
            return;
        }

        // Nyalakan UI HP Bar dan Teks HP sebelum Boss muncul
        if (bossHPBarUI != null) {
            bossHPBarUI.SetActive(true); 
            Debug.Log("UI BossHPBar berhasil diaktifkan oleh WaveManager.");
        }
        if (textBarBoss != null) {
            textBarBoss.gameObject.SetActive(true);
        }

        // Tentukan titik spawn
        Transform spawnPoint = bossSpawnPoint != null ? bossSpawnPoint : spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        GameObject boss = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
        
        // --- LOGIKA BARU: Ambil komponen Health dari clone Boss yang baru saja lahir ---
        activeBossHealth = boss.GetComponent<Health>();

        // Hubungkan sinyal mati Boss ke WaveManager
        boss.GetComponent<Health>().onDeath += OnEnemyDefeated;
        enemiesRemaining++;
    }

    void OnEnemyDefeated() {
        enemiesRemaining--;
        if (enemiesRemaining <= 0) {
            // Ketika boss atau semua musuh mati, bersihkan referensi health dan sembunyikan UI Teks
            activeBossHealth = null; 
            if (textBarBoss != null) textBarBoss.gameObject.SetActive(false);

            StartCoroutine(WaitBeforeOpeningShop());
        }
    }

    IEnumerator WaitBeforeOpeningShop() {
        Debug.Log("Wave Clear! Memberi waktu 5 detik untuk memungut item...");
        
        // Tunggu selama 5 detik (gameplay masih berjalan)
        yield return new WaitForSeconds(5f);

        // Setelah 5 detik, baru jalankan fungsi EndWave untuk buka shop
        EndWave();
    }

    void EndWave() {
        if (bossHPBarUI != null) bossHPBarUI.SetActive(false); // Pastikan slider HP utama ikut mati saat wave kelar
        shopPanel.SetActive(true); // Munculkan toko
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;       // Pause game
    }

    public void GoToNextWave() {
        shopPanel.SetActive(false);
        Time.timeScale = 1f;       // Resume game
        currentWaveIndex++;
        StartCoroutine(StartWave());
    }
}