using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    //public int hordeSize = 10;
    public float hordeSpawnDelay = 0.3f;

    [Header("Horde Warning")]
    public float hordeWarningTime = 3f; // Time between warning and actual spawn
    public float postHordePause = 5f;   // Time to wait after a horde before normal spawns resume
    public TextMeshProUGUI warningText; // Assign this in the inspector

    private float nextSpawnTime;
    private bool hordeInProgress = false;
    private bool isPausedAfterHorde = false;

    void Start()
    {
        nextSpawnTime = Time.time + timeBetweenSpawns;

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        HordeManager.Instance.OnHordeTriggered += () =>
        {
            StartCoroutine(HandleHordeWithWarning());
        };
    }

    void Update()
    {
        if (hordeInProgress || isPausedAfterHorde) return;

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
        Debug.Log("Instantiated: " + prefab);
    }

    private IEnumerator HandleHordeWithWarning()
    {
        hordeInProgress = true;

        // Show warning text
        if (warningText != null)
        {
            warningText.text = "A horde is incoming!";
            warningText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(hordeWarningTime);

        // Hide warning
        if (warningText != null)
            warningText.gameObject.SetActive(false);

        // Start spawning horde
        yield return StartCoroutine(SpawnHorde());

        // Pause normal spawns for a while
        isPausedAfterHorde = true;
        yield return new WaitForSeconds(postHordePause);
        isPausedAfterHorde = false;

        hordeInProgress = false;
    }

    private IEnumerator SpawnHorde()
    {
        for (int i = 0; i < Random.Range(8, 12); i++)
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
