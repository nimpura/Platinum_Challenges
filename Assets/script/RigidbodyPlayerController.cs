using UnityEngine;
using UnityEngine.InputSystem;

public class RigidbodyPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7.0f;
    private float currentMoveSpeed;

    [Header("Jump Settings")]
    public float jumpForce = 6.0f;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    private float jumpCooldown = 0f;

    [Header("Look Settings")]
    public float mouseSensitivity = 15.0f;
    public Transform cameraTransform;

    [Header("Ground Check")]
    public bool isGrounded;
    
    private Animator animator;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraVerticalRotation = 0f;
    private bool isRunning;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        GetInput();
        HandleLook();

        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }

        if (jumpCooldown <= 0)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            isGrounded = false;
        }

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        if (moveInput.magnitude > 0)
        {
            currentMoveSpeed = isRunning ? runSpeed : walkSpeed;

            float animatorSpeedValue = isRunning ? 2.0f : 1.0f;
            animator.SetFloat("Speed", animatorSpeedValue);
        }
        else
        {

            currentMoveSpeed = 0f;
            animator.SetFloat("Speed", 0f);
        }


        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            isGrounded = false;
            animator.SetBool("isGrounded", false);
            jumpCooldown = 0.15f;
        }
    }

    void FixedUpdate()
    {
        HandleMove();
    }

    void GetInput()
    {
        if (Keyboard.current != null)
        {
            float moveX = 0f;
            float moveZ = 0f;
            if (Keyboard.current.wKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
            moveInput = new Vector2(moveX, moveZ).normalized;


            isRunning = Keyboard.current.shiftKey.isPressed;
        }

        if (Mouse.current != null)
        {
            lookInput = Mouse.current.delta.ReadValue();
        }
    }

    void HandleMove()
    {
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;

        Vector3 targetVelocity = moveDir * currentMoveSpeed;

        Vector3 currentVelocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z);
    }

    void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        cameraVerticalRotation -= mouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
