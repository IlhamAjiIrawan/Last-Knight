using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour {
    public List<WaveData> allWaves;    // Masukkan file WaveData di sini
    public Transform[] spawnPoints;    // Lokasi muncul musuh
    public GameObject shopPanel;       // Panel Upgrade
    
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

        foreach (var group in wave.enemiesInWave) {
            for (int i = 0; i < group.count; i++) {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

    void SpawnEnemy(GameObject prefab) {
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(prefab, randomPoint.position, Quaternion.identity);
        
        // Hubungkan sinyal mati musuh ke fungsi di Manager ini
        enemy.GetComponent<Health>().onDeath += OnEnemyDefeated;
        enemiesRemaining++;
    }

    void OnEnemyDefeated() {
        enemiesRemaining--;
        if (enemiesRemaining <= 0) {
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