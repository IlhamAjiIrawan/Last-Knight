using UnityEngine;
using System.Collections;
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
}

