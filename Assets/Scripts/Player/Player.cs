using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;

public class Player : MonoBehaviour
{
    public Transform cam;

    [Header("Player Settings")]
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float climbingSpeed = 2f;
    [SerializeField] private float mass = 1f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float turnSmoothVelocity;

    public float Height
    {
        get => controller.height;
        set => controller.height = value;
    }

    public event Action OnBeforeMove;

    internal float movementSpeedMultiplier;

    State _state;

    public State CurrentState
    {
        get => _state;
        set
        {
            _state = value;
            velocity = Vector3.zero;
        }
    }

    public enum State
    {
        Walking,
        Climbing
    }

    // Inputs
    PlayerInput input;
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction crouchAction;

    CharacterController controller;

    private Vector2 look;
    internal Vector3 velocity;
    private Vector3 currentVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!input) input = GetComponent<PlayerInput>();
        moveAction = input.actions["Move"];
        lookAction = input.actions["Look"];
        jumpAction = input.actions["Jump"];
        crouchAction = input.actions["Crouch"];
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        movementSpeedMultiplier = 1f;
        switch (CurrentState)
        {
            case State.Walking:
                UpdateGravity();
                UpdateMovement();
                UpdateLook();
                break;
            case State.Climbing:
                UpdateClimbing();
                UpdateLook();
                break;
        }
    }

    void UpdateGravity()
    {
        var gravity = Physics.gravity * mass * Time.deltaTime;
        velocity.y = controller.isGrounded ? -1f : velocity.y + gravity.y;
    }

    void UpdateMovement()
    {
        movementSpeedMultiplier = 1f;
        OnBeforeMove?.Invoke();

        var moveInput = moveAction.ReadValue<Vector2>();
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        input *= movementSpeed * movementSpeedMultiplier;

        if (input.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            if (jumpAction.WasPressedThisFrame() && controller.isGrounded){
                velocity.y += jumpSpeed;
            }

            Vector3 targetVelocity = moveDir * movementSpeed;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
            controller.Move((currentVelocity + velocity) * Time.deltaTime);
        }
    }

    void UpdateClimbing()
    {
        var moveInput = moveAction.ReadValue<Vector2>();
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        input *= climbingSpeed;

        var factor = acceleration * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, input, factor);

        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateLook()
    {
        look.x += lookAction.ReadValue<Vector2>().x * mouseSensitivity;
        look.y += lookAction.ReadValue<Vector2>().y * mouseSensitivity;

        look.y = Mathf.Clamp(look.y, -90f, 90f);
    }
}
