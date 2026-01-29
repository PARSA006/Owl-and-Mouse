using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    [Header("Look Settings")]
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    [Header("Crouch Settings")]
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    private float baseWalkSpeed;
    private float baseRunSpeed;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private CharacterController characterController;

    private bool canMove = true;

    private static PlayerMovement instance;
    public static PlayerMovement Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        characterController = GetComponent<CharacterController>();

        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Priority 1: TeleportData (doors, portals)
        if (TeleportData.useTeleportPosition)
        {
            transform.position = TeleportData.spawnPosition;
            TeleportData.useTeleportPosition = false;
        }
        // Priority 2: Normal scene load (start game, load save)
        else if (SaveManager.HasSave() && !PlayerRespawn.restoredFromCheckpoint)
        {
            transform.position = SaveManager.LoadPlayerPosition();
        }
    }



    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        float movementDirectionY = moveDirection.y;
        moveDirection = forward * curSpeedX + right * curSpeedY;

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

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

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (!canMove) return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }
}
