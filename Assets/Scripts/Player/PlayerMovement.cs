using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

// Add new stances here like prone
public enum Stance
{
    Standing,
    Climbing,
    Jumping,
    Crouching
}

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterController _player;
    public PlayerInput _input;
    public Transform _playerObj;

    [Space]
    [Header("Settings")]
    [SerializeField] private LayerMask ceilingMask = ~0;

    [Space]
    [Header("Stance Settings")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1.2f;
    private float currentHeight;

    [Space]
    [Header("Movement Settings")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float crouchSpeed = 0.5f;
    [SerializeField] private float jumpSpeed = 6f;
    [SerializeField] private float climbingSpeed = 3f;
    
    [Space]
    [SerializeField] private float turnSmoothing = 0.25f;
    [SerializeField] private float moveDamping = 0.25f;

    [Space]
    [Header("Physics Settings")]
    [SerializeField] private float mass = 1f;


    public Stance stance = Stance.Standing;
    bool IsCrouching => standingHeight - currentHeight > .1f;
    private float turnSmoothingVelocity;
    Vector3 velocity;

    // --CHECKS--
    private bool wasGrounded;
    private bool requestedCrouch;

    void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _player = GetComponent<CharacterController>();
        _input.actions.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        standingHeight = _player.height;
        _player.center = new Vector3(0f, standingHeight / 2f, 0f);
    }

    private void Update()
    {
        UpdateGravity();
        UpdateMovement();
        switch (stance)
        {
            case Stance.Standing:
                UpdateMovement();
                UpdateGravity();
                break;
            case Stance.Climbing:
                UpdateClimbing();
                break;
        }
    }

    void UpdateGravity()
    {
        var gravity = Physics.gravity * mass * Time.deltaTime;
        velocity.y = _player.isGrounded ? -1f : velocity.y + gravity.y;
    }

    void UpdateMovement()
    {
        currentHeight = _player.height;

        // Player rotation
        var moveInput = _input.actions["Move"].ReadValue<Vector2>();
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        Vector3 horizontalMove = Vector3.zero;

        if (_input.actions["Crouch"].WasPressedThisFrame())
        {
            requestedCrouch = !requestedCrouch;
        }

        bool groundedBeforeMove = _player.isGrounded;

        if (_input.actions["Jump"].WasPressedThisFrame() && groundedBeforeMove && !requestedCrouch)
        {
            velocity.y = jumpSpeed;
            stance = Stance.Jumping;
        }

        if (inputDir.magnitude >= 0.1f)
        {
            float cameraYaw = Camera.main.transform.eulerAngles.y;
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraYaw;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothingVelocity, turnSmoothing);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = speed * (stance == Stance.Crouching ? crouchSpeed : 1);
            
            horizontalMove = moveDir.normalized * currentSpeed;
        }

        Vector3 motion = horizontalMove;
        motion.y = velocity.y;

        _player.Move(motion * Time.deltaTime);

        bool groundedAfterMove = _player.isGrounded;

        if (groundedAfterMove && !wasGrounded && stance == Stance.Jumping)
        {
            stance = Stance.Standing;
        }

        if (groundedAfterMove)
        {
            if (requestedCrouch && stance != Stance.Crouching) EnterCrouch();

            if (!requestedCrouch && stance == Stance.Crouching)
            {
                ExitCrouch();

                if (stance == Stance.Crouching) requestedCrouch = true;
            }
        }

        wasGrounded = groundedAfterMove;
    }

    // Setting the players stance and modifying the character based on the info for each stance, can be expanded with a new case and new stance in the enum
    private void EnterCrouch()
    {
        stance = Stance.Crouching;
        _player.height = crouchHeight;
        _player.center = new Vector3(0f, crouchHeight / 2f, 0f);
    }

    private void ExitCrouch()
    {
        if (HasCeilingAbove()) return;

        stance = Stance.Standing;
        _player.height = standingHeight;
        _player.center = new Vector3(0f, standingHeight / 2f, 0f);
    }

    private bool HasCeilingAbove()
    {
        Vector3 playerCenter = transform.TransformPoint(_player.center);

        float radius = _player.radius;
        float halfHeight = standingHeight * 0.5f;

        Vector3 bottom = playerCenter + Vector3.up * (radius - halfHeight);
        Vector3 top = playerCenter + Vector3.up * (halfHeight - radius);

        return Physics.CheckCapsule(bottom, top, radius, ceilingMask, QueryTriggerInteraction.Ignore);
    }

    private void UpdateClimbing()
    {
        var input = _input.actions["Move"].ReadValue<Vector2>();

        var climbDir = climbingSpeed * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, input, climbDir);

        _player.Move(velocity * climbDir);
    }

    private void OnDrawGizmosSelected()
    {
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Current Stance: " + stance);
    }
}