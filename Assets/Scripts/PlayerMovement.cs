using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Camera playerCamera;

    public float speed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float wallRunTime = 3f;
    public float wallJumpForce = 8f;
    public float cameraTiltAngle = 15f;
    public float mouseSensitivity = 100f;
    public float wallRunBufferTime = 0.2f; // Buffer time to allow wall run activation

    private Vector3 velocity;
    private bool isGrounded;
    private bool isWallRunning;
    private Vector3 wallNormal;
    private float wallRunTimer;
    private float xRotation = 0f;
    private float wallRunBuffer;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Move();
        HandleJumping();
        HandleMouseLook();
    }

    void Move()
    {
        if (isWallRunning)
        {
            velocity.y = 0; // Nullify gravity effect while wall running
            controller.Move(transform.forward * speed * Time.deltaTime);
            return;
        }

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity = Vector3.down * 2f; // Reset velocity when grounded
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isWallRunning)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                Jump();
            }
            else if (wallRunBuffer > 0) // Allow a small window to activate wall run
            {
                StartWallRun(wallNormal);
            }
        }
    }

    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void WallJump()
    {
        isWallRunning = false;
        wallRunTimer = 0;
        ResetCameraTilt();

        // Push the player away from the wall
        Vector3 jumpDirection = transform.forward + wallNormal; // Ensure proper movement away from the wall
        velocity = jumpDirection.normalized * wallJumpForce;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void StartWallRun(Vector3 normal)
    {
        if (isWallRunning || isGrounded) return; // Prevent wall running on ground

        isWallRunning = true;
        wallNormal = normal;
        wallRunTimer = wallRunTime;
        TiltCamera(-cameraTiltAngle * Mathf.Sign(normal.x)); // Fixed camera tilt direction
        wallRunBuffer = 0; // Reset buffer since wall run is now active

        StartCoroutine(WallRunCountdown());
    }

    IEnumerator WallRunCountdown()
    {
        while (wallRunTimer > 0)
        {
            wallRunTimer -= Time.deltaTime;
            yield return null;
        }
        isWallRunning = false;
        ResetCameraTilt();
    }

    void TiltCamera(float tiltAmount)
    {
        playerCamera.transform.localRotation = Quaternion.Euler(0, 0, tiltAmount);
    }

    void ResetCameraTilt()
    {
        playerCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, playerCamera.transform.localRotation.z);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void SetWallRunBuffer(Vector3 normal)
    {
        wallRunBuffer = wallRunBufferTime;
        wallNormal = normal;
    }
}