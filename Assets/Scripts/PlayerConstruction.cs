using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConstruction : MonoBehaviour
{
    [Header("Player Settings")]
    public CharacterController controller;
    public Camera playerCamera;
    public float speed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 100f;

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

    private Vector3 velocity;
    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleConstructionModeToggle();
        HandlePlayerMovement();
        HandleCameraLook();

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

    // ---------------------- PLAYER MOVEMENT ----------------------
    void HandlePlayerMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCameraLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
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
            previewRenderers = previewObject.GetComponentsInChildren<Renderer>();
            previewObject.GetComponentInChildren<Collider>().enabled = false;
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
        Collider[] colliders = Physics.OverlapBox(position, previewObject.transform.localScale / 2, Quaternion.identity, obstructionLayer);
        canPlaceObject = (colliders.Length == 0 && isInBuildingZone);

        foreach (Renderer renderer in previewRenderers)
        {
            renderer.material = canPlaceObject ? validPlacementMaterial : invalidPlacementMaterial;
        }
    }

    void PlaceObject()
    {
        Instantiate(buildableObjects[currentBuildIndex], previewObject.transform.position, previewObject.transform.rotation, buildParent);
        buildCount++;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BuildingZone"))
        {
            isInBuildingZone = true;
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
