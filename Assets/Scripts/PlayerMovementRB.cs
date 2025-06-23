using UnityEngine;

public class PlayerMovementRB : MonoBehaviour
{
    public Rigidbody rb;
    public Camera playerCamera;

    public float speed = 6f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 100f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    private bool isGrounded;
    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        Move();
        HandleJumping();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        Vector3 velocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

        rb.linearVelocity = velocity;
    }

    void HandleJumping()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down * 0.5f, groundCheckDistance, groundMask);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }
}