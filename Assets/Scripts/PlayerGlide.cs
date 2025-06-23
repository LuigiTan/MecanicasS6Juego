using System.Collections;
using UnityEngine;

public class PlayerGlide : MonoBehaviour
{
    public CharacterController controller;
    public Camera playerCamera;

    public float speed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float glideGravity = -2f;
    public float glideSpeed = 10f;
    public float glideAcceleration = 1.5f;
    public float maxGlideSpeed = 20f;
    public float minGlideSpeed = 5f;
    public float mouseSensitivity = 100f;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isGliding;
    private float xRotation = 0f;
    private float currentGlideSpeed;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        isGrounded = controller.isGrounded;

        if (isGrounded && isGliding)
        {
            isGliding = false;
        }

        if (isGliding)
        {
            Glide();
        }
        else
        {
            Move();
            HandleJumping();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleGlide();
        }
    }

    void Move()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity = Vector3.down * 2f;
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
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void Glide()
    {
        Vector3 forwardMove = transform.forward * currentGlideSpeed;

        // Use camera angle instead of input
        float pitch = xRotation; // Negative when looking down, positive when looking up

        if (pitch < -5f) // Looking down
        {
            currentGlideSpeed = Mathf.Clamp(currentGlideSpeed + glideAcceleration * Time.deltaTime, minGlideSpeed, maxGlideSpeed);
            velocity.y = Mathf.Lerp(velocity.y, glideGravity * 2f, Time.deltaTime * 2);
        }
        else if (pitch > 5f) // Looking up
        {
            currentGlideSpeed = Mathf.Clamp(currentGlideSpeed - glideAcceleration * Time.deltaTime, minGlideSpeed, maxGlideSpeed);
            velocity.y = Mathf.Lerp(velocity.y, Mathf.Abs(glideGravity) * -0.5f, Time.deltaTime * 2); // Move up
        }
        else
        {
            velocity.y = Mathf.Lerp(velocity.y, glideGravity, Time.deltaTime * 2);
        }

        controller.Move(forwardMove * Time.deltaTime + velocity * Time.deltaTime);
    }


    void ToggleGlide()
    {
        if (isGrounded) return;

        isGliding = !isGliding;
        if (isGliding)
        {
            currentGlideSpeed = glideSpeed;
            velocity.y = 0;
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
