using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // -------------------------
    // MOVEMENT SETTINGS
    // -------------------------

    [Header("Movement Settings")]
    public Camera playerCamera;      // The camera used for looking around
    public float walkSpeed = 6f;     // Normal walking speed
    public float runSpeed = 12f;     // Running speed
    public float jumpPower = 7f;     // Jump force
    public float gravity = 10f;      // Gravity applied when airborne

    [Header("Look Settings")]
    public float lookSpeed = 2f;     // Mouse sensitivity
    public float lookXLimit = 45f;   // Vertical look clamp

    [Header("Crouch Settings")]
    public float defaultHeight = 2f; // Standing height
    public float crouchHeight = 1f;  // Crouched height
    public float crouchSpeed = 3f;   // Movement speed while crouching

    private float baseWalkSpeed;     // Cached original walk speed
    private float baseRunSpeed;      // Cached original run speed

    private Vector3 moveDirection = Vector3.zero; // Current movement vector
    private float rotationX = 0f;                 // Vertical camera rotation
    private CharacterController characterController;

    private bool canMove = true;     // Used to disable movement (cutscenes, menus, etc.)

    // -------------------------
    // SINGLETON
    // -------------------------

    private static PlayerMovement instance;
    public static PlayerMovement Instance => instance;

    private void Awake()
    {
        // Singleton pattern: ensures only one PlayerMovement exists across scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Keep player object when switching scenes
        DontDestroyOnLoad(gameObject);

        characterController = GetComponent<CharacterController>();

        // Store original speeds for crouch reset
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;

        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Lock mouse cursor for FPS-style movement
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called whenever a new scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If we restored from a checkpoint, DO NOT override the player's position
        if (PlayerRespawn.restoredFromCheckpoint)
        {
            Debug.Log("PLAYERMOVEMENT: Skipping OnSceneLoaded position override (checkpoint restore)");
            return;
        }

        // Otherwise, load saved position from SaveManager (if it exists)
        if (SaveManager.HasSave())
        {
            transform.position = SaveManager.LoadPlayerPosition();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    // -------------------------
    // MOVEMENT LOGIC
    // -------------------------

    private void HandleMovement()
    {
        // Convert local directions to world space
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Movement input (WASD)
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        // Preserve vertical velocity (gravity/jump)
        float movementDirectionY = moveDirection.y;

        // Combine movement directions
        moveDirection = forward * curSpeedX + right * curSpeedY;

        // Jumping
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity when airborne
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // -------------------------
        // CROUCHING
        // -------------------------

        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = baseWalkSpeed;
            runSpeed = baseRunSpeed;
        }

        // Move the character controller
        characterController.Move(moveDirection * Time.deltaTime);
    }

    // -------------------------
    // CAMERA LOOK LOGIC
    // -------------------------

    private void HandleLook()
    {
        if (!canMove) return;

        // Vertical rotation (camera only)
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Horizontal rotation (player body)
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }
}
