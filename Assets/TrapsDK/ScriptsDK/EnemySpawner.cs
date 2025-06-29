using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnEntry
{
    public GameObject enemyPrefab;
    public int weight = 1;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public Transform spawnPoint;
    public List<SpawnEntry> enemyVariants;

    [Header("Timing")]
    public float timeBetweenSpawns = 3f;
    public float hordeInterval = 30f;
    public int hordeSize = 10;
    public float hordeSpawnDelay = 0.3f;

    private float nextSpawnTime;
    private float nextHordeTime;

    void Start()
    {
        nextSpawnTime = Time.time + timeBetweenSpawns;
        nextHordeTime = Time.time + hordeInterval;
    }

    void Update()
    {
        if (Time.time >= nextHordeTime)
        {
            StartCoroutine(SpawnHorde());
            nextHordeTime = Time.time + hordeInterval;
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            SpawnSingleEnemy();
            nextSpawnTime = Time.time + timeBetweenSpawns;
        }
    }

    private void SpawnSingleEnemy()
    {
        GameObject prefab = GetRandomEnemy();
        if (prefab != null)
            Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }

    private IEnumerator SpawnHorde()
    {
        for (int i = 0; i < hordeSize; i++)
        {
            SpawnSingleEnemy();
            yield return new WaitForSeconds(hordeSpawnDelay);
        }
    }

    private GameObject GetRandomEnemy()
    {
        if (enemyVariants.Count == 0) return null;

        int totalWeight = 0;
        foreach (var entry in enemyVariants)
            totalWeight += entry.weight;

        int randomValue = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var entry in enemyVariants)
        {
            cumulative += entry.weight;
            if (randomValue < cumulative)
                return entry.enemyPrefab;
        }

        return enemyVariants[enemyVariants.Count - 1].enemyPrefab; // fallback
    }
}
