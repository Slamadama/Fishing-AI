// First-person movement with walking, jumping and hand-rolled swimming; the CharacterController does no gravity of its own, so everything vertical happens here.
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float WalkSpeed = 5.0f;
    [SerializeField] private float RunningSpeed = 9.0f;

    [SerializeField] private float JumpForce = 9.0f;
    [SerializeField] private float Gravity = 20.0f;

    [Header("Swimming")]
    [SerializeField] private float WaterLevelY = 20.6f;
    [SerializeField] private float SwimEntryDepth = 1.0f;
    [SerializeField] private float SwimExitDepth = 0.8f;
    [SerializeField] private float SwimSpeed = 3.0f;
    [SerializeField] private float SwimRunSpeed = 5.0f;
    [SerializeField] private float SwimVerticalSpeed = 3.0f;

    [SerializeField] private float BuoyancyStrength = 3.0f;
    [SerializeField] private float MaxBuoyancySpeed = 2.5f;
    [SerializeField] private float WaterDrag = 4.0f;

    [SerializeField] private float FloatEyeAboveWater = 0.2f;
    [SerializeField] private float SurfaceBobHeight = 0.06f;
    [SerializeField] private float SurfaceBobSpeed = 1.3f;

    [SerializeField] private float LookSensitivity = 0.2f;
    [SerializeField] private float LookAngleLimit = 90f;

    private Camera mainCamera;
    private CharacterController characterController;

    private InputAction moveInput;
    private InputAction lookInput;
    private InputAction runInput;
    private InputAction jumpInput;

    private float currentMoveSpeed = 0.0f;
    private Vector3 moveDirection = Vector3.zero;
    private float lookAngle = 0.0f;

    private bool isSwimming = false;
    private Vector3 swimVelocity = Vector3.zero;

    public bool IsSwimming { get { return isSwimming; } }

    private void Start()
    {
        mainCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();

        moveInput = InputSystem.actions.FindAction("Move");
        lookInput = InputSystem.actions.FindAction("Look");
        runInput = InputSystem.actions.FindAction("Sprint");
        jumpInput = InputSystem.actions.FindAction("Jump");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentMoveSpeed = WalkSpeed;
    }

    private void Update()
    {
        Vector2 moveVector = moveInput.ReadValue<Vector2>();
        Vector2 lookVector = lookInput.ReadValue<Vector2>();

        HandleMovement(moveVector);
        HandleLook(lookVector);
    }

    private void HandleMovement(Vector2 moveVector)
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        UpdateWaterState();

        if (isSwimming)
        {
            HandleSwimming(moveVector, forward, right);
        }
        else
        {
            HandleWalking(moveVector, forward, right);
        }
    }

    private void UpdateWaterState()
    {
        float halfHeight = characterController.height * 0.5f;
        float feetY = transform.position.y - halfHeight;
        float submerged = WaterLevelY - feetY;

        // Two different depths on purpose (hysteresis): one number would flicker walking/swimming
        // on and off every frame while floating exactly at the surface.
        if (!isSwimming && submerged > SwimEntryDepth)
        {
            isSwimming = true;
            swimVelocity = new Vector3(moveDirection.x, 0.0f, moveDirection.z);
        }
        else if (isSwimming && submerged < SwimExitDepth && characterController.isGrounded)
        {
            isSwimming = false;
            swimVelocity = Vector3.zero;

            if (moveDirection.y > 0.0f)
            {
                moveDirection.y = 0.0f;
            }
        }

        // The isGrounded test is what makes leaving the water mean "waded ashore": without it, holding
        // Space in deep water would flip to walking, gravity would drop you, and the swim would restart
        // as a visible bounce.
    }

    private void HandleWalking(Vector2 moveVector, Vector3 forward, Vector3 right)
    {
        float oldY = moveDirection.y;

        currentMoveSpeed = runInput.IsPressed() ? RunningSpeed : WalkSpeed;

        Vector2 newSpeed = new Vector2(moveVector.y * currentMoveSpeed, moveVector.x * currentMoveSpeed);

        moveDirection = (forward * newSpeed.x) + (right * newSpeed.y);

        if (jumpInput.WasPressedThisFrame() && characterController.isGrounded)
        {
            moveDirection.y = JumpForce;
        }
        else
        {
            moveDirection.y = oldY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= Gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleSwimming(Vector2 moveVector, Vector3 forward, Vector3 right)
    {
        currentMoveSpeed = runInput.IsPressed() ? SwimRunSpeed : SwimSpeed;

        Vector3 wishVelocity = (forward * (moveVector.y * currentMoveSpeed)) + (right * (moveVector.x * currentMoveSpeed));

        // Lerp rather than snap is the water drag: release W and the player coasts to a stop.
        swimVelocity = Vector3.Lerp(swimVelocity, wishVelocity, Mathf.Clamp01(WaterDrag * Time.deltaTime));

        moveDirection.x = swimVelocity.x;
        moveDirection.z = swimVelocity.z;

        float verticalInput = 0.0f;

        if (jumpInput.IsPressed())
        {
            verticalInput += 1.0f;
        }

        if (DiveHeld())
        {
            verticalInput -= 1.0f;
        }

        if (verticalInput != 0.0f)
        {
            moveDirection.y = verticalInput * SwimVerticalSpeed;
        }
        else
        {
            float bob = Mathf.Sin(Time.time * SurfaceBobSpeed) * SurfaceBobHeight;

            float floatLineY = FloatLineY() + bob;

            float distanceToLine = floatLineY - transform.position.y;

            moveDirection.y = Mathf.Clamp(distanceToLine * BuoyancyStrength, -MaxBuoyancySpeed, MaxBuoyancySpeed);
        }

        // Swimming has no gravity, so without this ceiling holding Space would fly the player out of
        // the lake. The only way out is meant to be swimming to the beach.
        float halfHeight = characterController.height * 0.5f;
        float maxCenterY = WaterLevelY - SwimExitDepth + halfHeight - 0.2f;

        if (moveDirection.y > 0.0f && transform.position.y > maxCenterY)
        {
            moveDirection.y = 0.0f;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private float FloatLineY()
    {
        float cameraHeight = mainCamera.transform.localPosition.y;

        return WaterLevelY + FloatEyeAboveWater - cameraHeight;
    }

    private bool DiveHeld()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.leftCtrlKey.isPressed
            || Keyboard.current.cKey.isPressed;
    }

    private void HandleLook(Vector2 lookVector)
    {
        // Yaw turns the body so "forward" stays correct for movement; pitch tilts only the camera so
        // the CharacterController capsule never tips over.
        transform.Rotate(Vector3.up * (lookVector.x * LookSensitivity));

        lookAngle -= lookVector.y * LookSensitivity;
        lookAngle = Mathf.Clamp(lookAngle, -LookAngleLimit, LookAngleLimit);

        mainCamera.transform.localEulerAngles = new Vector3(lookAngle, 0f, 0f);
    }
}
