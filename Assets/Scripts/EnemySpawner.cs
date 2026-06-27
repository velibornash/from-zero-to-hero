using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject wolfPrefab;
    public GameObject barbarianPrefab;
    public Transform[] spawnPoints;

    public float initialDelay = 2f;
    public float waveInterval = 6f;
    public int enemiesPerWave = 3;
    public int maxConcurrentEnemies = 12;

    bool started;
    float timer;

    void Start()
    {
        SlotManager.SlotBuilt += OnSlotBuilt;
    }

    void OnDestroy()
    {
        SlotManager.SlotBuilt -= OnSlotBuilt;
    }

    void OnSlotBuilt(int index)
    {
        // Church (index 0) unlocks combat so the player can earn gold for the flag.
        if (index == 0 && !started)
        {
            started = true;
            HUDController.PushEvent("Enemies are coming from the forest! Defend the village!");
            InvokeRepeating(nameof(SpawnWave), initialDelay, waveInterval);
        }
    }

    void SpawnWave()
    {
        if (spawnPoints.Length == 0) return;

        int current = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude).Length;
        if (current >= maxConcurrentEnemies) return;

        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (current + i >= maxConcurrentEnemies) break;

            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            bool useBarbarian = Random.value > 0.55f;
            var prefab = useBarbarian ? barbarianPrefab : wolfPrefab;
            if (prefab == null) continue;

            var offset = Random.insideUnitSphere * 4f;
            offset.y = 0f;
            var spawnPos = point.position + offset;
            spawnPos.y = 0.5f;
            var enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            enemy.name = useBarbarian ? "Barbarian" : "Wolf";

            var e = enemy.GetComponent<Enemy>();
            if (e != null) e.enemyName = enemy.name;
        }
    }
}
