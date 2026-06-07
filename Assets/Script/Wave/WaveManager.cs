using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// --- SANGAT PENTING: Class pembantu agar bisa muncul rapi di Inspector ---
[System.Serializable]
public class BossDataSetup
{
    public GameObject bossPrefab;       // Tempat menaruh Prefab Boss
    public string bossDisplayName;      // Tempat mengetik Nama Kustom Boss untuk UI
}

public class WaveManager : MonoBehaviour {
    public List<WaveData> allWaves;
    public Transform[] spawnPoints;
    public GameObject shopPanel;  

    [Header("Audio Settings")]
    public AudioSource bgmSource;       
    public AudioClip normalWaveBGM;    
    public AudioClip bossWaveBGM;      

    [Header("Boss Wave Settings")]
    // --- DIUBAH: Sekarang menggunakan List dari class custom BossDataSetup ---
    public List<BossDataSetup> bossToSpawnList;  
    public Transform bossSpawnPoint;

    [Header("New Dynamic UI Settings")]
    public GameObject bossHPBarPrefab;    
    public Transform bossHPContainer;     

    private int currentWaveIndex = 0;
    private int enemiesRemaining = 0;

    void Start() {
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave() {
        if (currentWaveIndex >= allWaves.Count) {
            Debug.Log("Semua Wave Selesai!");
            yield break;
        }

        WaveData wave = allWaves[currentWaveIndex];

        if (wave.isBossWave) {
            SwitchBGM(bossWaveBGM);
        } else {
            SwitchBGM(normalWaveBGM);
        }

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
        
        enemy.GetComponent<Health>().onDeath += OnEnemyDefeated;
        enemiesRemaining++;
    }

    void SpawnBoss() {
        // Cek apakah list boss kosong
        if (bossToSpawnList == null || bossToSpawnList.Count == 0) {
            Debug.LogError("Gagal Spawn! List 'Boss To Spawn List' masih kosong di WaveManager Inspector.");
            return;
        }

        // Loop sebanyak data boss yang kamu daftarkan di Inspector
        for (int i = 0; i < bossToSpawnList.Count; i++) {
            // Validasi jika ada slot element yang lupa belum diisi prefab-nya
            if (bossToSpawnList[i] == null || bossToSpawnList[i].bossPrefab == null) continue;

            Transform spawnPoint = bossSpawnPoint != null ? bossSpawnPoint : spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            Vector3 spawnPosition = spawnPoint.position;
            if (i > 0) {
                spawnPosition += new Vector3(Random.Range(-3.5f, 3.5f), 0, Random.Range(-3.5f, 3.5f));
            }

            // 1. Lahirkan Boss menggunakan data Prefab dari list kustom
            GameObject boss = Instantiate(bossToSpawnList[i].bossPrefab, spawnPosition, Quaternion.identity);
            Health bossHealth = boss.GetComponent<Health>();

            // 2. Lahirkan UI Health Bar Khusus untuk Boss ini
            if (bossHPBarPrefab != null && bossHPContainer != null && bossHealth != null) {
                GameObject uiBar = Instantiate(bossHPBarPrefab, bossHPContainer);
                BossHPBar barScript = uiBar.GetComponent<BossHPBar>();
                
                if (barScript != null) {
                    // --- FITUR BARU: Mengirimkan nama kustom dari Inspector ke UI Bar masing-masing ---
                    string namaKustom = bossToSpawnList[i].bossDisplayName;
                    
                    // Jika kolom nama dikosongkan di inspector, otomatis pakai nama prefab asli sebagai cadangan
                    if (string.IsNullOrEmpty(namaKustom)) {
                        namaKustom = bossToSpawnList[i].bossPrefab.name;
                    }

                    barScript.Setup(bossHealth, namaKustom);
                }
            }

            boss.GetComponent<Health>().onDeath += OnEnemyDefeated;
            enemiesRemaining++;
        }
    }

    void OnEnemyDefeated() {
        enemiesRemaining--;
        if (enemiesRemaining <= 0) {
            StartCoroutine(WaitBeforeOpeningShop());
        }
    }

    IEnumerator WaitBeforeOpeningShop() {
        Debug.Log("Wave Clear! Memberi waktu 5 detik untuk memungut item...");
        yield return new WaitForSeconds(5f);
        EndWave();
    }

    void EndWave() {
        if (bossHPContainer != null) {
            foreach (Transform child in bossHPContainer) {
                Destroy(child.gameObject);
            }
        }

        shopPanel.SetActive(true); 
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;      
    }

    public void GoToNextWave() {
        shopPanel.SetActive(false);
        Time.timeScale = 1f;       
        currentWaveIndex++;
        StartCoroutine(StartWave());
    }

    private void SwitchBGM(AudioClip newClip) {
        if (bgmSource == null || newClip == null) return;
        if (bgmSource.clip == newClip && bgmSource.isPlaying) return;

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.loop = true; 
        bgmSource.Play();
    }
}