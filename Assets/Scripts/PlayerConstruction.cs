using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;

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
    private bool isInConstructionMode = false;
    private bool canPlaceObject = false;
    private bool isInBuildingZone = false;
    private int currentBuildIndex = 0;
    private int buildCount = 0;

    private Dictionary<GameObject, int> trapCounts = new Dictionary<GameObject, int>();

    [SerializeField] private TextMeshProUGUI upgradeText;
    [SerializeField] private TextMeshProUGUI upgradeLevelText;

    // Optional: limits per trap type (match buildableObjects index)
    public List<int> trapLimits = new List<int>();


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // Optional safety: default trap limit per type
        while (trapLimits.Count < buildableObjects.Count)
        {
            trapLimits.Add(maxBuildCount);
        }
    }


    void Update()
    {
        HandleConstructionModeToggle();

        if (isInConstructionMode)
        {
            HandleObjectPreview();
            HandleObjectSelection();

            if (Input.GetMouseButtonDown(0) && canPlaceObject && buildCount < maxBuildCount)
            {
                PlaceObject();
            }
        }
        //else
        //{
        //}
    }

    // ---------------------- CONSTRUCTION MODE ----------------------
    void HandleConstructionModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isInConstructionMode = !isInConstructionMode;

            //Cursor.lockState = isInConstructionMode ? CursorLockMode.None : CursorLockMode.Locked;

            if (isInConstructionMode)
            {
                CreatePreviewObject();
            }
            else
            {
                Destroy(previewObject);
            }
        }
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
            currentBuildIndex = (currentBuildIndex + 1) % buildableObjects.Count;
            CreatePreviewObject();
        }
        else if (scroll < 0)
        {
            currentBuildIndex = (currentBuildIndex - 1 + buildableObjects.Count) % buildableObjects.Count;
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
        int trapLimit = trapLimits.Count > currentBuildIndex ? trapLimits[currentBuildIndex] : maxBuildCount;
        int currentCount = trapCounts.ContainsKey(trapPrefab) ? trapCounts[trapPrefab] : 0;
        bool hasMoney = PlayerStats.Instance.money >= trap.cost;

        canPlaceObject = (colliders.Length == 0 && isInBuildingZone && hasMoney && currentCount < trapLimit);

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
            Bounds bounds = new Bounds(previewObject.transform.position, Vector3.zero);
            foreach (Renderer r in previewObject.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(r.bounds);
            }

            Gizmos.color = canPlaceObject ? Color.green : Color.red;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }


    void PlaceObject()
    {
        GameObject trapPrefab = buildableObjects[currentBuildIndex];
        TrapBase trapData = trapPrefab.GetComponent<TrapBase>();
        int trapLimit = trapLimits.Count > currentBuildIndex ? trapLimits[currentBuildIndex] : maxBuildCount;

        if (!trapCounts.ContainsKey(trapPrefab))
            trapCounts[trapPrefab] = 0;

        int currentCount = trapCounts[trapPrefab];

        if (trapData == null)
        {
            Debug.LogWarning("Trap prefab is missing TrapBase component.");
            return;
        }

        if (currentCount >= trapLimit)
        {
            Debug.Log("Trap limit reached for this type.");
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


        trapCounts[trapPrefab]++;
        buildCount++;

    }

    public void OnTrapDestroyed(TrapBase trap)
    {
        foreach (GameObject prefab in trapCounts.Keys)
        {
            TrapBase test = prefab.GetComponent<TrapBase>();
            if (test != null && test.GetType() == trap.GetType())
            {
                trapCounts[prefab] = Mathf.Max(0, trapCounts[prefab] - 1);
                buildCount = Mathf.Max(0, buildCount - 1);
                return;
            }
        }
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
}
