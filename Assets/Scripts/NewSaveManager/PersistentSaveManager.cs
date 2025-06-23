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
    public List<PlacedObjectData> placedObjects = new List<PlacedObjectData>();
}

[System.Serializable]
public class PlacedObjectData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
}

public class PersistentSaveManager : MonoBehaviour
{
    public static PersistentSaveManager Instance { get; private set; }

    public Transform player;
    public CharacterController controller;
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
            playerRotation = player.rotation
        };

        Debug.Log($"[SAVE] Saved player position: {data.playerPosition}");
        Debug.Log($"[SAVE] Saved player rotation: {data.playerRotation.eulerAngles}");

        if (constructionScript != null && constructionScript.buildParent != null)
        {
            Debug.Log("[SAVE] Gathering constructed objects...");
            foreach (Transform child in constructionScript.buildParent)
            {
                
                data.placedObjects.Add(new PlacedObjectData
                {
                    prefabName = child.name.Replace("(Clone)", "").Trim(),
                    position = child.position,
                    rotation = child.rotation
                });
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
            controller = player.GetComponent<CharacterController>();
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
            controller = player.GetComponent<CharacterController>();

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
                    Instantiate(prefab, obj.position, obj.rotation, constructionScript.buildParent);
                }
                else
                {
                    Debug.LogWarning($"[LOAD] Prefab not found: {obj.prefabName}");
                }
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
