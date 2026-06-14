using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class BossDataSetup
{
    public GameObject bossPrefab;       
    public string bossDisplayName;      
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
    public List<BossDataSetup> bossToSpawnList;  
    public Transform bossSpawnPoint;

    [Header("New Dynamic UI Settings")]
    public GameObject bossHPBarPrefab;    
    public Transform bossHPContainer;    

    [Header("Optimization Settings")]
    [Tooltip("Jumlah maksimal musuh yang boleh ada di map secara bersamaan")]
    public int maxActiveEnemies = 10; 

    private int currentWaveIndex = 0;
    private int enemiesRemaining = 0;       
    private int totalWaveEnemiesRemaining = 0;

    void Start() {
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave() {
        if (currentWaveIndex >= allWaves.Count) {
            Debug.Log("Semua Wave Selesai!");
            yield break;
        }

        WaveData wave = allWaves[currentWaveIndex];

        totalWaveEnemiesRemaining = 0;
        foreach (var group in wave.enemiesInWave) {
            totalWaveEnemiesRemaining += group.count;
        }
        if (wave.isBossWave) {
            totalWaveEnemiesRemaining += bossToSpawnList.Count;
        }

        if (wave.isBossWave) {
            SwitchBGM(bossWaveBGM);
        } else {
            SwitchBGM(normalWaveBGM);
        }

        // --- PROSES SPAWN MUSUH NORMAL ---
        foreach (var group in wave.enemiesInWave) {
            for (int i = 0; i < group.count; i++) {
                
                // Batasan maksimal musuh aktif di layar
                while (enemiesRemaining >= maxActiveEnemies) {
                    yield return null; 
                }

                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        // --- PROSES SPAWN BOSS ---
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
        if (bossToSpawnList == null || bossToSpawnList.Count == 0) {
            Debug.LogError("Gagal Spawn! List 'Boss To Spawn List' masih kosong.");
            return;
        }

        // Tracking Pasangan 1 (Barbarian & Stone Dragon)
        KingBarbarian barbarianScript = null;
        Health dragonHealth = null;

        // TRACKING PASANGAN 2: ASSASSIN & FOREST DRAGON
        AssassinBossAI assassinScript = null;
        Health forestDragonHealth = null;

        for (int i = 0; i < bossToSpawnList.Count; i++) {
            if (bossToSpawnList[i] == null || bossToSpawnList[i].bossPrefab == null) continue;

            Transform spawnPoint = bossSpawnPoint != null ? bossSpawnPoint : spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 spawnPosition = spawnPoint.position;
            
            if (i > 0) {
                spawnPosition += new Vector3(Random.Range(-3.5f, 3.5f), 0, Random.Range(-3.5f, 3.5f));
            }

            GameObject boss = Instantiate(bossToSpawnList[i].bossPrefab, spawnPosition, Quaternion.identity);
            Health bossHealth = boss.GetComponent<Health>();

            // Validasi Pasangan Lama
            if (boss.GetComponent<KingBarbarian>() != null) {
                barbarianScript = boss.GetComponent<KingBarbarian>();
            }
            if (boss.GetComponent<StoneDragon>() != null) {
                dragonHealth = bossHealth;
            }

            if (boss.GetComponent<AssassinBossAI>() != null) {
                assassinScript = boss.GetComponent<AssassinBossAI>();
            }
            if (boss.GetComponent<ForestDragon>() != null) {
                forestDragonHealth = bossHealth;
            }

            if (bossHPBarPrefab != null && bossHPContainer != null && bossHealth != null) {
                GameObject uiBar = Instantiate(bossHPBarPrefab, bossHPContainer);
                BossHPBar barScript = uiBar.GetComponent<BossHPBar>();
                
                if (barScript != null) {
                    string namaKustom = bossToSpawnList[i].bossDisplayName;
                    if (string.IsNullOrEmpty(namaKustom)) {
                        namaKustom = bossToSpawnList[i].bossPrefab.name;
                    }
                    barScript.Setup(bossHealth, namaKustom);
                }
            }

            boss.GetComponent<Health>().onDeath += OnEnemyDefeated;
            enemiesRemaining++; 
        }

        // Eksekusi Link Pasangan Lama
        if (barbarianScript != null && dragonHealth != null) {
            barbarianScript.otherBossHealth = dragonHealth;
            Debug.Log("<color=green>🔗 [WaveManager]: Berhasil menjodohkan King Barbarian dengan Stone Dragon secara Real-Time!</color>");
        }

        if (assassinScript != null && forestDragonHealth != null) {
            assassinScript.otherBossHealth = forestDragonHealth;
            Debug.Log("<color=purple>🔮 [WaveManager]: Berhasil menjodohkan Assassin Boss dengan Forest Dragon secara Real-Time!</color>");
        }
    }

    void OnEnemyDefeated() {
        enemiesRemaining--;          
        totalWaveEnemiesRemaining--; 

        if (totalWaveEnemiesRemaining <= 0) {
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