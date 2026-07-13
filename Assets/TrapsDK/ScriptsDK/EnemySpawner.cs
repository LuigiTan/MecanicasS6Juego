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

    [Header("Preview Enemies")]
    public List<SpawnEntry> previewEnemyVariants;

    [Header("Path")]
    public EnemyPath assignedPath;

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

    private bool combatActive = false;

    [Header("Preview Settings")]
    public float previewLifetime = 8f;
    public int maxPreviewEnemies = 5;

    private readonly List<GameObject> previewEnemies = new();
    public Material previewMAT;

    void Start()
    {
        nextSpawnTime = Time.time + timeBetweenSpawns;

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        HordeManager.Instance.OnHordeTriggered += () =>
        {
            StartCoroutine(HandleHordeWithWarning());
        };

        WavePhaseManager.Instance.OnCombatPhaseStarted += () =>
        {
            combatActive = true;
            ClearPreviewEnemies();
        };

        WavePhaseManager.Instance.OnBuildPhaseStarted += () =>
        {
            combatActive = false;
        };
    }

    void Update()
    {
        if (combatActive)
        {
            if (hordeInProgress || isPausedAfterHorde)
                return;

            if (Time.time >= nextSpawnTime)
            {
                SpawnSingleEnemy(enemyVariants);
                nextSpawnTime = Time.time + timeBetweenSpawns;
            }
        }
        else if (WavePhaseManager.Instance.IsBuildPhase)
        {
            if (Time.time >= nextSpawnTime)
            {
                SpawnSingleEnemy(previewEnemyVariants, true);
                nextSpawnTime = Time.time + timeBetweenSpawns;
            }
        }
    }


    private void SpawnSingleEnemy(List<SpawnEntry> list, bool isPreview = false)
    {
        GameObject prefab = GetRandomEnemy(list);

        if (prefab == null)
            return;

        GameObject enemyObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
            enemy.SetPath(assignedPath);

        if (isPreview)
        {
            enemyObj.tag = "Preview";
            ApplyPreviewMaterial(enemyObj);

            previewEnemies.Add(enemyObj);

            Destroy(enemyObj, previewLifetime);
            StartCoroutine(RemovePreviewReference(enemyObj));

            if (previewEnemies.Count > maxPreviewEnemies)
            {
                Destroy(previewEnemies[0]);
                previewEnemies.RemoveAt(0);
            }
        }
        else
        {
            EnemyTracker.Instance.RegisterEnemy();
        }
    }

    private void ApplyPreviewMaterial(GameObject enemy)
    {
        Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.material = previewMAT;
        }
    }

    private IEnumerator RemovePreviewReference(GameObject enemy)
    {
        yield return new WaitForSeconds(previewLifetime);

        if (previewEnemies.Contains(enemy))
            previewEnemies.Remove(enemy);
    }

    private void ClearPreviewEnemies()
    {
        foreach (GameObject enemy in previewEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        previewEnemies.Clear();
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

        // Horde finished spawning.
        // Don't resume normal spawning.
        hordeInProgress = false;
        combatActive = false;
    }

    private IEnumerator SpawnHorde()
    {
        for (int i = 0; i < Random.Range(8, 12); i++)
        {
            SpawnSingleEnemy(enemyVariants);
            yield return new WaitForSeconds(hordeSpawnDelay);
        }
    }

    private GameObject GetRandomEnemy(List<SpawnEntry> list)
    {
        if (list.Count == 0)
            return null;

        int totalWeight = 0;

        foreach (var entry in list)
            totalWeight += entry.weight;

        int random = Random.Range(0, totalWeight);

        int cumulative = 0;

        foreach (var entry in list)
        {
            cumulative += entry.weight;

            if (random < cumulative)
                return entry.enemyPrefab;
        }

        return list[list.Count - 1].enemyPrefab;
    }
}
