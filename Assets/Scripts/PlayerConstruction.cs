using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class PlayerConstruction : MonoBehaviour
{
    [Header("Player Settings")]
    public Camera playerCamera;

    [Header("Building Settings")]
    public List<GameObject> buildableObjects;
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;
    public LayerMask buildableLayer;
    public LayerMask obstructionLayer;
    public Transform buildParent;
    public int maxBuildCount = 10;

    private GameObject previewObject;
    private Renderer[] previewRenderers;
    private bool isInConstructionMode = true;
    private bool canPlaceObject = false;
    private bool isInBuildingZone = false;
    private int currentBuildIndex = 0;
    private int buildCount = 0;

    private Dictionary<GameObject, int> trapCounts = new Dictionary<GameObject, int>();

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI upgradeText;
    [SerializeField] private TextMeshProUGUI upgradeLevelText;
    [SerializeField] private TextMeshProUGUI TrapCountText;
    [SerializeField] private Image upgradeTextBG;
    [SerializeField] private Image upgradeLevelTextBG;

    [SerializeField] private TextMeshProUGUI buildingPhaseText;
    [SerializeField] private TextMeshProUGUI StartWaveText;
    [SerializeField] private Image buildingPhaseBG;
    [SerializeField] private Image StartWaveBG;

    [Header("Trap UI Icons")]
    public List<Image> trapIcons;
    public Color unlockedColor = Color.white;
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    public List<GameObject> buildingZones;

    [Header("Build Count Progression")]
    public int buildIncreaseEvery = 5;
    public int buildIncreaseAmount = 2;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        isInConstructionMode = true;
        CreatePreviewObject();

        WavePhaseManager.Instance.OnBuildPhaseStarted += EnterBuildPhase;
        WavePhaseManager.Instance.OnCombatPhaseStarted += ExitBuildPhase;

        HordeManager.Instance.OnWaveStarted += HandleWaveStarted;

        UpdateTrapCountUI();
    }


    void Update()
    {
        HandleConstructionModeToggle();
        UpdateTrapIcons();

        if (isInConstructionMode)
        {
            HandleObjectPreview();
            HandleObjectSelection();


            if (Input.GetMouseButtonDown(0) && canPlaceObject && buildCount < maxBuildCount)
            {
                PlaceObject();
            }
        }
    }


    // ---------------------- CONSTRUCTION MODE ----------------------
    void HandleConstructionModeToggle()
{
    if (Input.GetKeyDown(KeyCode.Q))
    {
        if (WavePhaseManager.Instance.IsBuildPhase)
        {
            WavePhaseManager.Instance.StartCombatPhase();
        }
    }
}

    private void EnterBuildPhase()
    {
        isInConstructionMode = true;

        if (previewObject == null)
            CreatePreviewObject();

        upgradeText.gameObject.SetActive(true);
        upgradeTextBG.gameObject.SetActive(true);
        upgradeLevelText.gameObject.SetActive(true);
        upgradeLevelTextBG.gameObject.SetActive(true);

        buildingPhaseText.gameObject.SetActive(true);
        buildingPhaseBG.gameObject.SetActive(true);
        StartWaveText.gameObject.SetActive(true);
        StartWaveBG.gameObject.SetActive(true);

        SetBuildingZonesVisible(true);
    }

    private void ExitBuildPhase()
    {
        isInConstructionMode = false;

        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        upgradeText.gameObject.SetActive(false);
        upgradeTextBG.gameObject.SetActive(false);
        upgradeLevelText.gameObject.SetActive(false);
        upgradeLevelTextBG.gameObject.SetActive(false);

        buildingPhaseText.gameObject.SetActive(false);
        buildingPhaseBG.gameObject.SetActive(false);
        StartWaveText.gameObject.SetActive(false);
        StartWaveBG.gameObject.SetActive(false);

        SetBuildingZonesVisible(false);
    }

    void CreatePreviewObject()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        if (buildableObjects.Count > 0)
        {
            previewObject = Instantiate(buildableObjects[currentBuildIndex]);

            // Disable all colliders (not just one)
            Collider[] colliders = previewObject.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            // Destroy any Rigidbody components to prevent physics interference
            Rigidbody[] rigidbodies = previewObject.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies)
            {
                Destroy(rb);
            }

            // Move preview to neutral physics layer
            SetLayerRecursively(previewObject, LayerMask.NameToLayer("Ignore Raycast"));

            // Get renderers for material change
            previewRenderers = previewObject.GetComponentsInChildren<Renderer>();
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }


    void HandleObjectSelection()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0)
        {
            int startIndex = currentBuildIndex;
            do
            {
                currentBuildIndex = (currentBuildIndex + 1) % buildableObjects.Count;
            }
            while (!IsTrapUnlocked(currentBuildIndex) && currentBuildIndex != startIndex);

            CreatePreviewObject();
        }
        else if (scroll < 0)
        {
            int startIndex = currentBuildIndex;
            do
            {
                currentBuildIndex = (currentBuildIndex - 1 + buildableObjects.Count) % buildableObjects.Count;
            }
            while (!IsTrapUnlocked(currentBuildIndex) && currentBuildIndex != startIndex);

            CreatePreviewObject();
        }
    }


    void HandleObjectPreview()
    {
        if (previewObject == null) return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f, buildableLayer))
        {
            previewObject.transform.position = hit.point;
            previewObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            CheckPlacementValidity(hit.point);
        }
    }

    void CheckPlacementValidity(Vector3 position)
    {
        // Calculate accurate world bounds from all renderers
        Bounds bounds = new Bounds(previewObject.transform.position, Vector3.zero);
        foreach (Renderer r in previewRenderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        // Use bounds center and extents to define overlap box
        Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, obstructionLayer);

        GameObject trapPrefab = buildableObjects[currentBuildIndex];
        TrapBase trap = trapPrefab.GetComponent<TrapBase>();
        bool hasMoney = PlayerStats.Instance.money >= trap.cost;

        canPlaceObject = (colliders.Length == 0 && isInBuildingZone && hasMoney && buildCount < maxBuildCount);

        foreach (Renderer renderer in previewRenderers)
        {
            renderer.material = canPlaceObject ? validPlacementMaterial : invalidPlacementMaterial;
        }

        if (!canPlaceObject && colliders.Length > 0)
        {
            Debug.Log("Blocked by: " + colliders[0].name);
        }

    }


    void OnDrawGizmos()
    {
        if (previewObject != null)
        {
            BoxCollider placement = previewObject.GetComponentInChildren<BoxCollider>();

            Vector3 center = placement.bounds.center;
            Vector3 extents = placement.bounds.extents;

            Collider[] colliders = Physics.OverlapBox(
                center,
                extents,
                placement.transform.rotation,
                obstructionLayer);

            Gizmos.color = canPlaceObject ? Color.green : Color.red;
            Gizmos.DrawWireCube(placement.center, placement.size);
        }
    }


    void PlaceObject()
    {
        if (!IsTrapUnlocked(currentBuildIndex))
        {
            Debug.Log("This trap is not yet unlocked based on horde count.");
            return;
        }

        GameObject trapPrefab = buildableObjects[currentBuildIndex];
        TrapBase trapData = trapPrefab.GetComponent<TrapBase>();

        if (trapData == null)
        {
            Debug.LogWarning("Trap prefab is missing TrapBase component.");
            return;
        }

        if (!PlayerStats.Instance.SpendMoney(trapData.cost))
        {
            Debug.Log("Not enough money to place trap.");
            return;
        }

        // Instantiate and assign UI
        GameObject placed = Instantiate(trapPrefab, previewObject.transform.position, previewObject.transform.rotation, buildParent);
        TrapBase placedTrap = placed.GetComponent<TrapBase>();

        // Assign UI references here
        placedTrap.upgradeText = upgradeText;
        placedTrap.upgradeLevelText = upgradeLevelText;

        buildCount++;

        UpdateTrapCountUI();
    }

    public void OnTrapDestroyed(TrapBase trap)
    {
        buildCount = Mathf.Max(0, buildCount - 1);
        UpdateTrapCountUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BuildingZone"))
        {
            isInBuildingZone = true;
            Debug.Log("isInBuildingZone = " + isInBuildingZone);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BuildingZone"))
        {
            isInBuildingZone = false;
        }
    }

    private bool IsTrapUnlocked(int index)
    {
        int currentHorde = HordeManager.Instance != null ? HordeManager.Instance.HordeCount : 0;

        if (index == 0) return true; // First prefab always available
        if (index == 1) return currentHorde >= 5;
        if (index == 2) return currentHorde >= 10;

        // Extend as needed for more prefabs
        return false;
    }

    private void UpdateTrapIcons()
    {
        int hordeCount = HordeManager.Instance != null ? HordeManager.Instance.HordeCount : 0;

        for (int i = 0; i < trapIcons.Count; i++)
        {
            bool unlocked = IsTrapUnlocked(i);

            if (trapIcons[i] != null)
            {
                if (unlocked)
                {
                    trapIcons[i].color = unlocked ? unlockedColor : lockedColor;
                    trapIcons[i].transform.GetChild(0).gameObject.SetActive(false);
                    trapIcons[i].transform.GetChild(1).gameObject.SetActive(false);
                }
            }
        }
    }

    private void SetBuildingZonesVisible(bool visible)
    {
        foreach (GameObject zone in buildingZones)
        {
            if (zone != null)
                zone.SetActive(visible);
        }
    }

    private void HandleWaveStarted(int wave)
    {
        if (wave % buildIncreaseEvery == 0)
        {
            maxBuildCount += buildIncreaseAmount;

            UpdateTrapCountUI();

            Debug.Log($"New build limit: {maxBuildCount}");
        }
    }

    private void OnDestroy()
    {
        if (HordeManager.Instance != null)
            HordeManager.Instance.OnWaveStarted -= HandleWaveStarted;
    }

    private void UpdateTrapCountUI()
    {
        if (TrapCountText != null)
        {
            TrapCountText.text = $"Turrets: {buildCount} / {maxBuildCount}";
        }
    }
}
