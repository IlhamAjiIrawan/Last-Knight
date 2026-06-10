using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyGroup
{
    public GameObject enemyPrefab; // Jenis musuh
    public int count; // Jumlah musuh jenis ini
}

[CreateAssetMenu(fileName = "New Wave", menuName = "Wave System/Wave")]
public class WaveData : ScriptableObject
{
    public List<EnemyGroup> enemiesInWave;
    public float spawnInterval = 1.5f;
    public bool isBossWave;

    [Header("Optimization Settings")]
    [Tooltip("Maksimal musuh aktif di scene secara bersamaan. Jika penuh, spawn akan ditunda.")]
    public int maxActiveEnemies = 10; // Default 10 unit sesuai request kamu
}