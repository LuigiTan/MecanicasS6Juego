using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Hook : MonoBehaviour
{
    public Camera playerCamera; // Cámara desde la que se dispara
    public float moveForce = 5f; // Fuerza con la que se acercan los objetos
    public LayerMask shootableLayer; // Capas de objetos que pueden ser disparados
    public LineRenderer raycastLinePrefab; // Prefab del LineRenderer
    public Image crosshair; // Imagen del crosshair

    public GameObject firstObject = null;
    public GameObject secondObject = null;
    private LineRenderer raycastLine = null;
    private Coroutine moveCoroutine = null;

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Disparo con clic izquierdo o gatillo
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, shootableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (firstObject == null)
            {
                firstObject = hitObject; // Primer objeto espera
            }
            else if (secondObject == null)
            {
                secondObject = hitObject;
                CreateLineRenderer(); // Crear línea entre objetos
                StartMovement(); // Comenzar acercamiento
            }
            else
            {
                ResetMovement(hitObject);
            }
        }
    }

    void CreateLineRenderer()
    {
        if (raycastLine == null && firstObject != null)
        {
            raycastLine = Instantiate(raycastLinePrefab, firstObject.transform);
            raycastLine.positionCount = 2;
            UpdateLineRenderer();
        }
    }

    void UpdateLineRenderer()
    {
        if (raycastLine != null && firstObject != null && secondObject != null)
        {
            raycastLine.SetPosition(0, firstObject.transform.position);
            raycastLine.SetPosition(1, secondObject.transform.position);
        }
    }

    void StartMovement()
    {
        if (firstObject != null && secondObject != null)
        {
            moveCoroutine = StartCoroutine(MoveObjects());
        }
    }

    IEnumerator MoveObjects()
    {
        Rigidbody rbSecond = secondObject.GetComponent<Rigidbody>();
        if (rbSecond == null) yield break;

        rbSecond.linearVelocity = Vector3.zero; // Evita acumulación de velocidad previa
        float timer = 0f;
        while (timer < 3f)
        {
            if (firstObject == null || secondObject == null) yield break;

            // Dirección hacia el primer objeto
            Vector3 direction = (firstObject.transform.position - secondObject.transform.position).normalized;
            rbSecond.AddForce(direction * moveForce, ForceMode.Acceleration);

            UpdateLineRenderer(); // Actualizar línea mientras se mueven
            timer += Time.deltaTime;
            yield return null;
        }

        // Detener el movimiento después de 3 segundos
        rbSecond.linearVelocity = Vector3.zero;
        Destroy(raycastLine.gameObject); // Eliminar la línea después del movimiento
    }

    void ResetMovement(GameObject newFirstObject)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        if (secondObject != null)
        {
            Rigidbody rbSecond = secondObject.GetComponent<Rigidbody>();
            if (rbSecond != null)
            {
                rbSecond.linearVelocity = Vector3.zero;
            }
        }

        if (raycastLine != null)
        {
            Destroy(raycastLine.gameObject); // Eliminar línea anterior
        }

        firstObject = newFirstObject;
        secondObject = null;
    }
}

public class CollisionHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Hook shootingSystem = FindObjectOfType<Hook>();
        if (shootingSystem != null && shootingSystem.secondObject == gameObject && collision.gameObject == shootingSystem.firstObject)
        {
            Rigidbody rbSecond = shootingSystem.secondObject.GetComponent<Rigidbody>();
            if (rbSecond != null)
            {
                rbSecond.linearVelocity = Vector3.zero;
            }
        }
    }
}
