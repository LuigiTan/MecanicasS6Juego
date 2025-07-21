using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public List<PlacedObjectData> placedObjects = new();
    public List<EnemyData> enemies = new(); // NEW
    public int hordeCount;
}

[System.Serializable]
public class EnemyData
{
    public Vector3 position;
    public Quaternion rotation;
    public string enemyType; // if you have multiple types of enemies
}


[System.Serializable]
public class PlacedObjectData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;

    public int level;             // Save level
    public float attackCooldown;  // Save upgraded attack cooldown
    public float activationRadius; // Save upgraded radius
}


public class PersistentSaveManager : MonoBehaviour
{
    public static PersistentSaveManager Instance { get; private set; }

    public Transform player;
    public PlayerMovementRB controller;
    public PlayerConstruction constructionScript;

    public string saveFileName = "saveData.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private SaveData pendingLoadData = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            SaveGame();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            LoadGame();
        }
    }

    public void SaveGame()
    {

        UpdateReferences();

        if (player == null)
        {
            Debug.LogError("[SAVE] Player transform not assigned. Cannot save.");
            return;
        }

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            playerPosition = player.position,
            playerRotation = player.rotation,
            hordeCount = HordeManager.Instance != null ? HordeManager.Instance.HordeCount : 0
        };

        Debug.Log($"[SAVE] Saved player position: {data.playerPosition}");
        Debug.Log($"[SAVE] Saved player rotation: {data.playerRotation.eulerAngles}");

        if (constructionScript != null && constructionScript.buildParent != null)
        {
            Debug.Log("[SAVE] Gathering constructed objects...");
            foreach (Transform child in constructionScript.buildParent)
            {
                TrapBase trap = child.GetComponent<TrapBase>();

                PlacedObjectData trapData = new PlacedObjectData
                {
                    prefabName = child.name.Replace("(Clone)", "").Trim(),
                    position = child.position,
                    rotation = child.rotation,
                    level = trap != null ? trap.level : 1,
                    attackCooldown = trap != null ? trap.attackCooldown : 1f,
                    activationRadius = trap != null ? trap.activationRadius : 5f
                };

                data.placedObjects.Add(trapData);
            }

            Debug.Log($"[SAVE] Saved {data.placedObjects.Count} constructed objects.");
        }
        else
        {
            Debug.LogWarning("[SAVE] Construction script or buildParent is null. No constructed objects saved.");
        }
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SAVE] Game saved at: {SavePath}");

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            data.enemies.Add(new EnemyData
            {
                position = enemy.transform.position,
                rotation = enemy.transform.rotation,
                enemyType = enemy.name.Replace("(Clone)", "").Trim()
            });
        }
        Debug.Log($"[SAVE] Saved {data.enemies.Count} enemies.");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[LOAD] No save file found.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        pendingLoadData = JsonUtility.FromJson<SaveData>(json);

        if (pendingLoadData.sceneName != SceneManager.GetActiveScene().name)
        {
            SceneManager.LoadScene(pendingLoadData.sceneName);
        }
        else
        {
            ApplyLoadedData();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateReferences();

        if (pendingLoadData != null)
        {
            StartCoroutine(DelayedApplyLoadedData());
        }

    }

    private void UpdateReferences()
    {
        // Buscar jugador si no está asignado
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
                Debug.Log("[UPDATE] Player reference updated.");
            }
            else
            {
                Debug.LogWarning("[UPDATE] Player not found in this scene.");
            }
        }

        // Buscar CharacterController si se puede
        if (player != null && controller == null)
        {
            controller = player.GetComponent<PlayerMovementRB>();
            if (controller != null)
                Debug.Log("[UPDATE] CharacterController found.");
        }

        // Buscar sistema de construcción si no está
        if (constructionScript == null)
        {
            constructionScript = FindAnyObjectByType<PlayerConstruction>();
            if (constructionScript != null)
                Debug.Log("[UPDATE] Construction system found.");
        }
    }


    private void ApplyLoadedData()
    {
        if (HordeManager.Instance != null)
        {
            HordeManager.Instance.SetHordeCount(pendingLoadData.hordeCount);
            Debug.Log($"[LOAD] Restored Horde Count: {pendingLoadData.hordeCount}");
        }


        // Buscar referencias en la nueva escena
        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null)
                player = found.transform;
        }

        if (constructionScript == null)
            constructionScript = Object.FindFirstObjectByType<PlayerConstruction>();
        if (controller == null && player != null)
            controller = player.GetComponent<PlayerMovementRB>();

        if (player == null)
        {
            Debug.LogError("[LOAD] Player Transform not found. Cannot restore.");
            return;
        }

        Debug.Log($"[LOAD] Restoring position: {pendingLoadData.playerPosition}");
        Debug.Log($"[LOAD] Restoring rotation: {pendingLoadData.playerRotation.eulerAngles}");

        if (controller != null) controller.enabled = false;
        player.position = pendingLoadData.playerPosition;
        player.rotation = pendingLoadData.playerRotation;
        if (controller != null) controller.enabled = true;

        // Restaurar objetos construidos si hay sistema
        if (constructionScript != null && constructionScript.buildParent != null)
        {
            foreach (Transform child in constructionScript.buildParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var obj in pendingLoadData.placedObjects)
            {
                GameObject prefab = constructionScript.buildableObjects.Find(p => p.name == obj.prefabName);
                if (prefab != null)
                {
                    GameObject trapObj = Instantiate(prefab, obj.position, obj.rotation, constructionScript.buildParent);
                    TrapBase trap = trapObj.GetComponent<TrapBase>();

                    if (trap != null)
                    {
                        trap.level = obj.level;
                        trap.attackCooldown = obj.attackCooldown;
                        trap.activationRadius = obj.activationRadius;

                        SphereCollider col = trap.GetComponent<SphereCollider>();
                        if (col != null)
                            col.radius = obj.activationRadius;
                    }
                }
            }
        }

        foreach (var enemyData in pendingLoadData.enemies)
        {
            GameObject enemyPrefab = EnemyManager.Instance.GetEnemyPrefab(enemyData.enemyType); // You need to implement this
            if (enemyPrefab != null)
            {
                Instantiate(enemyPrefab, enemyData.position, enemyData.rotation);
            }
            else
            {
                Debug.LogWarning($"[LOAD] Enemy prefab not found: {enemyData.enemyType}");
            }
        }


        Debug.Log("[LOAD] Game successfully restored.");
        pendingLoadData = null;
    }

    private IEnumerator DelayedApplyLoadedData()
    {
        // Esperar hasta que los objetos necesarios estén listos
        while (constructionScript != null && constructionScript.buildableObjects.Count == 0)
        {
            yield return null; // Espera 1 frame
        }

        UpdateReferences(); // Asegura que tenemos las referencias correctas

        ApplyLoadedData();
    }

}
